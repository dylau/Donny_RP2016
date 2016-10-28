using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace RP1516
{
    public class GhcGeometricSort : GH_Component
    {

        public GhcGeometricSort()
          : base("GeometricSort", "GeometricSort",
              "GeometricSort",
              "RP1516", "SyntaxGeneration")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Sorting Key", "Sorting Key", "Sorting Key", GH_ParamAccess.item);
            pManager.AddGenericParameter("All Fibers", "All Fibers", "All Fibers", GH_ParamAccess.list);
            pManager.AddNumberParameter("Sag Threshold", "Sag Threshold", "Sag Threshold", GH_ParamAccess.item, 0.1); // uesd for removing top-top fibers, in meter
            pManager.AddNumberParameter("Sag Fiber Density", "Sag Fiber Density", "Sag Fiber Density", GH_ParamAccess.item, 0.1); // a double between 0 and 1
            pManager.AddNumberParameter("Curliness Threshold", "Curliness Threshold", "Curliness Threshold", GH_ParamAccess.item, 0.5);
            //pManager.AddGenericParameter("Structure Input", "Structure Input", "A list of SoFiNode objects", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Sorted Fiber Curves", "Sorted Fiber Curves", "Sorted Fiber Curves", GH_ParamAccess.list); // all sorted fiber crvs
            pManager.AddGenericParameter("Sorted Custom Fibers", "Sorted Custom Fibers", "Sorted Custom Fibers", GH_ParamAccess.list); // for XML
            pManager.AddNumberParameter("Sorting Key Values", "Sorting Key Values", "Sorting Key Values", GH_ParamAccess.list);

            pManager.AddCurveParameter("N", "N", "Negative", GH_ParamAccess.list);
            pManager.AddCurveParameter("P", "P", "Positive", GH_ParamAccess.list);
            pManager.AddCurveParameter("NC", "NC", "Nega Curly", GH_ParamAccess.list);
            pManager.AddCurveParameter("PC", "PC", "Posi Curly", GH_ParamAccess.list);
            pManager.AddCurveParameter("NS", "NS", "Nega Straight", GH_ParamAccess.list);
            pManager.AddCurveParameter("PS", "PS", "Posi Straight", GH_ParamAccess.list);

            pManager.AddGenericParameter("Skipped", "Skipped", "Skipped", GH_ParamAccess.list);
            //pManager.AddNumberParameter("Info", "Skipped", "Skipped", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Fiber> AllPossibleFibers = new List<Fiber>();
            DA.GetDataList("All Fibers", AllPossibleFibers);  // big list

            string SortingFactor = null;
            DA.GetData<string>("Sorting Key", ref SortingFactor); // sorting rule

            double sagThreshold = 0.1;
            DA.GetData<double>("Sag Threshold", ref sagThreshold); // sag threshold 

            double NegativeFiberDensity = 1.0;
            DA.GetData<double>("Sag Fiber Density", ref NegativeFiberDensity); // density

            double curlyThreshold = 0.5;
            DA.GetData<double>("Curliness Threshold", ref curlyThreshold); // curliness threshold 

            // -------------------------------reduce negative---------------------------------
            List<Fiber> SkippedFibers = new List<Fiber>();
            List<Fiber> RemainingFibers = new List<Fiber>();

            // - / +
            List<Fiber> NegativeFibers = AllPossibleFibers.Where(o => o.Type == "Negative").ToList(); // all negative fibers
            List<Fiber> PositiveFibers = AllPossibleFibers.Except(NegativeFibers).ToList(); //all positive fibers

            // narrow down -
            SkippedFibers.AddRange(NegativeFibers.Where(o => o.PinB.Index < 9 || o.PinB.Index > 17));
            NegativeFibers.RemoveAll(o => o.PinB.Index < 9 || o.PinB.Index > 17);

            // curliness 
            // NC
            List<Fiber> NegaCurly = NegativeFibers
                .OrderByDescending(o => o.Curliness)
                .Take((int)(NegativeFibers.Count * curlyThreshold))
                .ToList();

            // NS
            List<Fiber> NegaStraight = NegativeFibers
                .Except(NegaCurly)
                .ToList();                   

            // PC
            List<Fiber> PosiCurly = PositiveFibers
                .OrderByDescending(o => o.Curliness)
                .Take((int)(PositiveFibers.Count * curlyThreshold))
                .ToList(); 

            // PS
            List<Fiber> PosiStraight = PositiveFibers.Except(PosiCurly).ToList();                                                  
            
            // reduce density
            if (NegativeFiberDensity == 0) // density 0 
            {
                SkippedFibers.AddRange(NegaCurly);
                RemainingFibers = AllPossibleFibers.Except(SkippedFibers).ToList();
            }

            if (NegativeFiberDensity > 0 && NegativeFiberDensity < 1) // density 0 ~ 1
            {
                NegaCurly.OrderBy(o => o.PinA.Position.Z).ToList();

                // ============================================================================
                int skipCount = (int)Math.Floor(1 / NegativeFiberDensity);
                for (int i = 0; i < (int)((double)NegaCurly.Count / (double)skipCount); i++)
                {
                    for (int j = 0; j < skipCount - 1; j++) //remove
                    {
                        SkippedFibers.Add(NegaCurly[skipCount * i + j]);
                        NegaCurly.Remove(NegaCurly[skipCount * i + j]);
                    }
                    // remain
                }
                // ============================================================================

                RemainingFibers = AllPossibleFibers.Except(SkippedFibers).ToList();
            }

            // nega curly reduced

            List<Fiber> RemainingNegativeFibers = NegaCurly.Concat(NegaStraight).ToList(); // remaining nega
            // -------------------------------------------- Sorting --------------------------------------------

            // output parameters
            List<Fiber> SortedFibers = new List<Fiber>();
            List<Curve> SortedFiberCrvs = new List<Curve>();
            List<double> SortingKeyValues = new List<double>();
            List<string> FiberTypes = new List<string>();


            #region: Relative height
            /*
            if (SortingFactor == "Relative Height")
            {
                // find out realtive hight betweenn all fibers
                for (int i = 0; i < AllPossibleFibers.Count - 1; i++)
                {
                    for (int j = i + 1; j < AllPossibleFibers.Count; j++) 
                    {
                        AllPossibleFibers[i].DetermineRelativeHeight(AllPossibleFibers[j]);
                    }
                }

                // sort
                SortedFibers.Add(AllPossibleFibers[0]);
                for (int i = 1; i < AllPossibleFibers.Count; i++)
                {
                    for (int j = 0; j < SortedFibers.Count; j++) // sorted fibers from high to low
                    {
                        int index = -1;
                        //AllPossibleFibers
                        //AllPossibleFibers[i].IsHigher
                    }
                }
            }
            */

            #endregion // not finished
            #region: Length difference
            /*
            if (SortingFactor == "Length difference")
            {
                AllPossibleFibers.ForEach(o => o.CalculateLengthDifference());
                SortedFibers = AllPossibleFibers.OrderBy(o => o.LengthDifference).ToList();
                SortedFiberCrvs = SortedFibers.Select(o => o.FiberCrv).ToList();
                SortingKeyValues = SortedFibers.Select(o => o.LengthDifference).ToList();
            }
            */

            #endregion
            #region: Pin position
            if (SortingFactor == "Pin position")
            {
                // calculate the sorting key value
                AllPossibleFibers.ForEach(o => o.CalculatePinPotential());
                // sorting
                SortedFibers = AllPossibleFibers.OrderByDescending(o => o.PinPotential).ToList();

                for (int i = 0; i < SortedFibers.Count; i++) // change fiber sorting ID
                {
                    SortedFibers[i].FiberSortingIndex = i;
                }

                SortedFiberCrvs = SortedFibers.Select(o => o.FiberCrv).ToList();
                SortingKeyValues = SortedFibers.Select(o => o.PinPotential).ToList();
            }
            #endregion
            #region: Potential
            /*
            if (SortingFactor == "Potential")
            {
                AllPossibleFibers.ForEach(o => o.CalculatePotential());
                SortedFibers = AllPossibleFibers.OrderByDescending(o => o.Potential).ToList();
                SortedFiberCrvs = SortedFibers.Select(o => o.FiberCrv).ToList();
                SortingKeyValues = SortedFibers.Select(o => o.Potential).ToList();
            }
            */
            #endregion
            #region: Fiber type
            /*
            if (SortingFactor == "Fiber type")
            {
                AllPossibleFibers.ForEach(o => o.FindTuringPtOnCrv());
                AllPossibleFibers.ForEach(o => o.CalculateLengthDifference());

                List<Fiber> ChainFibers = new List<Fiber>();
                List<Fiber> DoublyFibers = new List<Fiber>();
                List<Fiber> ArchFibers = new List<Fiber>();
                foreach (Fiber ifiber in AllPossibleFibers)
                {
                    if (ifiber.Type == "Chain")
                        ChainFibers.Add(ifiber);
                    if (ifiber.Type == "Arch")
                        ArchFibers.Add(ifiber);
                    if (ifiber.Type == "Doubly")
                        DoublyFibers.Add(ifiber);
                }

                List<Fiber> SortedChainFibers = new List<Fiber>();
                List<Fiber> SortedDoublyFibers = new List<Fiber>();
                List<Fiber> SortedArchFibers = new List<Fiber>();
                SortedChainFibers = ChainFibers.OrderBy(o => o.LengthDifference).ToList();
                SortedDoublyFibers = DoublyFibers.OrderBy(o => o.LengthDifference).ToList();
                SortedArchFibers = ArchFibers.OrderBy(o => o.LengthDifference).ToList();

                SortedFibers = SortedChainFibers.Concat(SortedDoublyFibers).Concat(SortedArchFibers).ToList();
                SortedFiberCrvs = SortedFibers.Select(o => o.FiberCrv).ToList();
                SortingKeyValues = SortedFibers.Select(o => o.LengthDifference).ToList();

                FiberTypes = SortedFibers.Select(o => o.Type).ToList();
            }
            */
            #endregion
            #region: +-Curvature
            if (SortingFactor == "+-Curvature") // determine chain and nonchain, sorted by verticle distances.
            {

                // compute SagThreshold
                double maxDistanceMax = NegativeFibers.Select(o => o.Curliness).Max();
                double minDistanceMin = NegativeFibers.Select(o => o.Curliness).Min();
                double SagThreshold = minDistanceMin + (maxDistanceMax - minDistanceMin) * sagThreshold;

                List<Fiber> ChainFibers = new List<Fiber>();
                List<Fiber> NonChainfibers = new List<Fiber>();
                List<Fiber> SortedChains = new List<Fiber>();
                List<Fiber> SortedNonChains = new List<Fiber>();
                List<Line> FiberStraightLines = new List<Line>();

                AllPossibleFibers.ForEach(o => o.DetermineChain());

                foreach (Fiber ifiber in AllPossibleFibers) // grouping chains and nonchains
                {
                    if (ifiber.Type == "Chain")
                        ChainFibers.Add(ifiber);
                    else
                    {
                        NonChainfibers.Add(ifiber);
                    }
                }

                SortedChains = ChainFibers.OrderByDescending(o => o.MaximumDistance).ToList(); // sorting chains: max -> min

                if (sagThreshold > 0)
                {
                    SortedChains.Select(o => o.MaximumDistance > SagThreshold);
                    //List<Fiber> SkippedFibers = new List<Fiber>();

                }
                    
                SortedChains.RemoveAll(o => o.MaximumDistance > SagThreshold); // remove top-top fibers, threshold default = 0.1m

                SortedNonChains = NonChainfibers.OrderByDescending(o => o.PinPotential).ToList(); // sorting nonchains: min -> max

                SortedFibers = SortedChains.Concat(SortedNonChains).ToList(); // merging into a big fiber list

                for (int i = 0; i < SortedFibers.Count; i++) // change fiber sorting ID
                {
                    SortedFibers[i].FiberSortingIndex = i;
                }

                SortedFiberCrvs = SortedFibers.Select(o => o.FiberCrv).ToList();
                SortingKeyValues = SortedFibers.Select(o => (double)o.FiberSortingIndex).ToList();
                FiberStraightLines = SortedFibers.Select(o => o.StraightLine).ToList();
                FiberTypes = SortedFibers.Select(o => o.Type).ToList();

            }
            #endregion 

            #region: Curliness
            if (SortingFactor == "Curliness")
            {
                double curlyMax = 0.0;
                double curlyMin = 0.0;
                double curlyValue = 0.0;
                curlyMax = RemainingFibers.Select(o => o.Curliness).Max();
                curlyMin = RemainingFibers.Select(o => o.Curliness).Min();
                curlyValue = curlyMin + (curlyMax - curlyMin) * curlyThreshold;

                NegaCurly.OrderBy(o => o.Curliness).ToList(); // curly -> stragiht
                NegaStraight.OrderBy(o => o.Curliness).ToList(); // curly -> stragiht
                PosiStraight.OrderByDescending(o => o.Curliness).ToList(); // stragiht -> curly
                PosiCurly.OrderByDescending(o => o.Curliness).ToList(); // straight ->curly

                SortedFibers = NegaCurly
                    .Concat(NegaStraight)
                    .Concat(PosiStraight)
                    .Concat(PosiCurly)
                    .ToList();

                

                for (int i = 0; i < SortedFibers.Count; i++) // change fiber sorting ID
                {
                    SortedFibers[i].FiberSortingIndex = i;
                }
                
                SortedFiberCrvs = SortedFibers.Select(o => o.FiberCrv).ToList();
                SortingKeyValues = SortedFibers.Select(o => o.MaximumDistance).ToList();

            }
            #endregion

            // ===================================================================
            DA.SetDataList("Sorted Custom Fibers", SortedFibers);
            DA.SetDataList("Sorted Fiber Curves", SortedFiberCrvs);
            DA.SetDataList("Sorting Key Values", SortingKeyValues);

            DA.SetDataList("N", NegativeFibers.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("P", PositiveFibers.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("NC", NegaCurly.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("PC", PosiCurly.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("NS", NegaStraight.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("PS", PosiStraight.Select(o => o.FiberCrv).ToList());

            DA.SetDataList("Skipped", SkippedFibers.Select(o => o.FiberCrv).ToList());
            //DA.SetDataList("Info", info);

            //DA.GetDataList<SoFiNode>("Structure Input", SoFiNodes);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{09668d3e-e1ce-4ce6-a663-e4d7ec008d84}"); }
        }
    }

}