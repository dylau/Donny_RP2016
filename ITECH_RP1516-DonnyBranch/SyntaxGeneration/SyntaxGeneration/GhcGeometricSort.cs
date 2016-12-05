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
            pManager.AddGenericParameter("All Fibers", "All Fibers", "All Fibers", GH_ParamAccess.list);
            pManager.AddNumberParameter("negative threshold", "bigger meaning less NC, more NS", "bigger meaning less NC, more NS", GH_ParamAccess.item);
            pManager.AddNumberParameter("positive threshold", "bigger meaning less PC, more PS", "bigger meaning less PC, more PS", GH_ParamAccess.item);
            pManager.AddNumberParameter("density NC", "density NC", "density NC", GH_ParamAccess.item);
            pManager.AddNumberParameter("density NS", "density NS", "density NS", GH_ParamAccess.item);
            pManager.AddNumberParameter("density PC", "density PC", "density PC", GH_ParamAccess.item);
            pManager.AddNumberParameter("density PS", "density PS", "density PS", GH_ParamAccess.item);
            pManager.AddNumberParameter("density Mix", "density Mix", "density Mix", GH_ParamAccess.item); 

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("N", "N", "Negative", GH_ParamAccess.list);
            pManager.AddCurveParameter("P", "P", "Positive", GH_ParamAccess.list);

            pManager.AddCurveParameter("NC", "NC", "Nega Curly", GH_ParamAccess.list);
            pManager.AddGenericParameter("NC*", "NC*", "Nega Curly object", GH_ParamAccess.list);
            pManager.AddCurveParameter("NC_skip", "NC_skip", "skipped Nega Curly", GH_ParamAccess.list);

            pManager.AddCurveParameter("NS", "NS", "Nega Straight", GH_ParamAccess.list);
            pManager.AddGenericParameter("NS*", "NS*", "Nega Straight object", GH_ParamAccess.list);
            pManager.AddCurveParameter("NS_skip", "NS_skip", "skipped Nega Straight", GH_ParamAccess.list);

            pManager.AddCurveParameter("PC", "PC", "Posi Curly", GH_ParamAccess.list);
            pManager.AddGenericParameter("PC*", "PC*", "Posi Curly object", GH_ParamAccess.list);
            pManager.AddCurveParameter("PC_skip", "PC_skip", "skipped Posi Curly", GH_ParamAccess.list);

            pManager.AddCurveParameter("PS", "PS", "Posi Straight", GH_ParamAccess.list);
            pManager.AddGenericParameter("PS*", "PS*", "Posi Straight object", GH_ParamAccess.list);
            pManager.AddCurveParameter("PS_skip", "PS_skip", "skipped Posi Straight", GH_ParamAccess.list);

            pManager.AddCurveParameter("Mix", "Mix", "Mix curvature", GH_ParamAccess.list);
            pManager.AddGenericParameter("Mix*", "Mix*", "Mix*", GH_ParamAccess.list);
            pManager.AddCurveParameter("Mix_skip", "Mix_skip", "skipped Mix", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Fiber> AllPossibleFibers = new List<Fiber>();
            DA.GetDataList("All Fibers", AllPossibleFibers);  // big list

            double thre_negative = 0.5; // curvature threshold for negative fibers; bigger meaning less NC, more NS
            DA.GetData("negative threshold", ref thre_negative);
            double thre_positive = 0.5; // curvature threshold for positive fibers; bigger meaning less PC, more PS
            DA.GetData("positive threshold", ref thre_positive);

            //density, default 0.5
            double den_NC = 0.5;
            DA.GetData("density NC", ref den_NC);
            double den_NS = 0.5;
            DA.GetData("density NS", ref den_NS);
            double den_PC = 0.5;
            DA.GetData("density PC", ref den_PC);
            double den_PS = 0.5;
            DA.GetData("density PS", ref den_PS);
            double den_Mix = 0.5;
            DA.GetData("density Mix", ref den_Mix);

            // ------------------------------- N/P dispatch ---------------------------------
            List<Fiber> NC_all = new List<Fiber>();
            List<Fiber> NS_all = new List<Fiber>();
            List<Fiber> PC_all = new List<Fiber>();
            List<Fiber> PS_all = new List<Fiber>();

            List<Fiber> NC_skip = new List<Fiber>();
            List<Fiber> NC = new List<Fiber>();
            List<Fiber> NS_skip = new List<Fiber>();
            List<Fiber> NS = new List<Fiber>();
            List<Fiber> PC_skip = new List<Fiber>();
            List<Fiber> PC = new List<Fiber>();
            List<Fiber> PS_skip = new List<Fiber>();
            List<Fiber> PS = new List<Fiber>();

            List<Fiber> Mix = new List<Fiber>();
            List<Fiber> Mix_skip = new List<Fiber>();
            // N / P
            List<Fiber> NegativeFibers = AllPossibleFibers.Where(o => o.Type == "Negative").ToList(); // all negative fibers
            List<Fiber> PositiveFibers = AllPossibleFibers.Where(o => o.Type == "Positive").ToList(); // all negative fibers

            List<Fiber> MixFibers_all = AllPossibleFibers.Except(NegativeFibers).Except(PositiveFibers).ToList(); //all positive fibers
            //MixFibers_all.OrderBy(o => o.Curliness).ToList(); // from straight to curve

            // ------------------------------- curvature dispatch ---------------------------------
            // N
            double curvatureMax_N = AllPossibleFibers.Select(o => o.Curliness).Max();
            double curvatureMin_N = AllPossibleFibers.Select(o => o.Curliness).Min();
            double curvatureThreshold_N = curvatureMin_N + (curvatureMax_N - curvatureMin_N) * thre_negative;

            // P
            double curvatureMax_P = AllPossibleFibers.Select(o => o.Curliness).Max();
            double curvatureMin_P = AllPossibleFibers.Select(o => o.Curliness).Min();
            double curvatureThreshold_P = curvatureMin_P + (curvatureMax_P - curvatureMin_P) * thre_positive;

            // NC
            NC_all = NegativeFibers
                //.OrderByDescending(o => o.Curliness)
                .Where(o => o.Curliness > curvatureThreshold_N)
                //.Take((int)(NegativeFibers.Count * thre_negative))
                .ToList();

            // NS
            NS_all = NegativeFibers
                .Except(NC_all)
                .ToList();                   

            // PC
            PC_all = PositiveFibers
                //.OrderByDescending(o => o.Curliness)
                .Where(o => o.Curliness > curvatureThreshold_P)
                //.Take((int)(PositiveFibers.Count * thre_positive))
                .ToList(); 

            // PS
            PS_all = PositiveFibers
                .Except(PC_all)
                .ToList();

            // ------------------------------- density dispatch ---------------------------------
            // NC 
            NC_all.OrderBy(o => o.PinA.Position.Z).ToList();
            int groupCount_NC = (int)Math.Floor(1 / den_NC); // e.g. den_NC = 0.2, take 1 (skip 4) every 5.
            for (int i = 0; i < (int)( (double)NC_all.Count / (double)groupCount_NC ); i = i + groupCount_NC)
            {
                NC.Add(NC_all[ i * groupCount_NC ]);
                for (int j = 1; j < groupCount_NC; j++)
                {
                    NC_skip.Add(NC_all[ i * groupCount_NC + j ]);
                }
            }

            // NS 
            NS_all.OrderBy(o => o.PinA.Position.Z).ToList();
            int groupCount_NS = (int)Math.Floor(1 / den_NS); 
            for (int i = 0; i < (int)((double)NS_all.Count / (double)groupCount_NS); i = i + groupCount_NS)
            {
                NS.Add(NS_all[i * groupCount_NS]);
                for (int j = 1; j < groupCount_NS; j++)
                {
                    NS_skip.Add(NS_all[i * groupCount_NS + j]);
                }
            }

            // PC 
            PC_all.OrderBy(o => o.PinA.Position.Z).ToList();
            int groupCount_PC = (int)Math.Floor(1 / den_PC); 
            for (int i = 0; i < (int)((double)PC_all.Count / (double)groupCount_PC); i = i + groupCount_PC)
            {
                PC.Add(PC_all[i * groupCount_PC]);
                for (int j = 1; j < groupCount_PC; j++)
                {
                    PC_skip.Add(PC_all[i * groupCount_PC + j]);
                }
            }

            // PS 
            PS_all.OrderBy(o => o.PinA.Position.Z).ToList();
            int groupCount_PS = (int)Math.Floor(1 / den_PS); 
            for (int i = 0; i < (int)((double)PS_all.Count / (double)groupCount_PS); i = i + groupCount_PS)
            {
                PS.Add(PS_all[i * groupCount_PS]);
                for (int j = 1; j < groupCount_PS; j++)
                {
                    PS_skip.Add(PS_all[i * groupCount_PS + j]);
                }
            }

            // Mix
            MixFibers_all.OrderBy(o => o.PinA.Position.Z).ToList();
            int groupCount_Mix = (int)Math.Floor(1 / den_Mix);
            for (int i = 0; i < (int)((double)MixFibers_all.Count / (double)groupCount_Mix); i = i + groupCount_Mix)
            {
                Mix.Add(MixFibers_all[i * groupCount_Mix]);
                for (int j = 1; j < groupCount_Mix; j++)
                {
                    Mix_skip.Add(MixFibers_all[i * groupCount_Mix + j]);
                }
            }


            // -------------------------------------------- Sorting --------------------------------------------
            // output parameters
            List<Fiber> SortedFibers = new List<Fiber>();
            List<Curve> SortedFiberCrvs = new List<Curve>();
            List<double> SortingKeyValues = new List<double>();
            List<string> FiberTypes = new List<string>();

            NC.OrderBy(o => o.Curliness).ToList(); // curve -> stragiht
            NS.OrderBy(o => o.Curliness).ToList(); // curve -> stragiht
            PS.OrderByDescending(o => o.Curliness).ToList(); // stragiht -> curve
            PC.OrderByDescending(o => o.Curliness).ToList(); // straight -> curve
            Mix.OrderBy(o => o.Curliness).ToList(); // sttaight -> curve

            // change fiber sorting ID
            for (int i = 0; i < NC.Count; i++)
                NC[i].FiberSortingIndex = i;
            for (int i = 0; i < NS.Count; i++) 
                NS[i].FiberSortingIndex = i;
            for (int i = 0; i < PS.Count; i++) 
                PS[i].FiberSortingIndex = i;
            for (int i = 0; i < PC.Count; i++) 
                PC[i].FiberSortingIndex = i;
            for (int i = 0; i < Mix.Count; i++) 
                Mix[i].FiberSortingIndex = i;
            
            // -------------------------------------------- Output --------------------------------------------
            DA.SetDataList("N", NegativeFibers.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("P", PositiveFibers.Select(o => o.FiberCrv).ToList());

            DA.SetDataList("NC", NC.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("NC*", NC);
            DA.SetDataList("NC_skip", NC_skip.Select(o => o.FiberCrv).ToList());

            DA.SetDataList("NS", NS.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("NS*", NS);
            DA.SetDataList("NS_skip", NS_skip.Select(o => o.FiberCrv).ToList());

            DA.SetDataList("PC", PC.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("PC*", PC);
            DA.SetDataList("PC_skip", PC_skip.Select(o => o.FiberCrv).ToList());

            DA.SetDataList("PS", PS.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("PS*", PS);
            DA.SetDataList("PS_skip", PS_skip.Select(o => o.FiberCrv).ToList());

            DA.SetDataList("Mix", Mix.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("Mix*", Mix);
            DA.SetDataList("Mix_skip", Mix_skip.Select(o => o.FiberCrv).ToList());
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