﻿using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using RP2k1516;


namespace RP1516
{
    public class GhcPathSelection : GH_Component
    {
        public GhcPathSelection()
          : base("PathSelection", "PathSelection",
              "PathSelection",
              "RP1516", "SyntaxGeneration")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Fiber set 1", "Fiber set 1", "Fiber set 1", GH_ParamAccess.list);
            pManager.AddGenericParameter("Fiber set 2", "Fiber set 2", "Fiber set 2", GH_ParamAccess.list);
            pManager.AddGenericParameter("Fiber set 3", "Fiber set 3", "Fiber set 3", GH_ParamAccess.list);
            pManager.AddGenericParameter("Fiber set 4", "Fiber set 4", "Fiber set 4", GH_ParamAccess.list);
            pManager.AddNumberParameter("Pin Capacity", "Pin Capacity", "Pin Capacity", GH_ParamAccess.item); // maximum pin capacity

            //pManager.AddNumberParameter("Order Tolerance", "Order Tolerance", "Order Tolerance", GH_ParamAccess.item); // how many fibers is it allowed to go back in the sorted list?
            //pManager.AddNumberParameter("Duplicate Maximum", "Duplicate Maximum", "Duplicate Maximum", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Fiber set 1 sequenced", "Fiber set 1 sequenced", "Fiber set 1 sequenced", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fiber set 2 sequenced", "Fiber set 2 sequenced", "Fiber set 2 sequenced", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fiber set 3 sequenced", "Fiber set 3 sequenced", "Fiber set 3 sequenced", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fiber set 4 sequenced", "Fiber set 4 sequenced", "Fiber set 4 sequenced", GH_ParamAccess.list);


            pManager.AddGenericParameter("Custom Fibers", "Custom Fibers", "Custom Fibers", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fiber Curves", "Fiber Curves", "Fiber Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("Skipped Fiber Curves", "Skipped Fiber Curves", "Skipped Fiber Curves to achieve continuity", GH_ParamAccess.list);
            pManager.AddTextParameter("Syntax Note", "Syntax Note", "Syntax Note", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fiber Sorting Indexes", "Fiber Sorting Indexes", "Fiber Sorting Indexes", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Fiber> FiberSet1 = new List<Fiber>();
            DA.GetDataList<Fiber>("Fiber set 1", FiberSet1);
            List<Fiber> FiberSet2 = new List<Fiber>();
            DA.GetDataList<Fiber>("Fiber set 2", FiberSet2);
            List<Fiber> FiberSet3 = new List<Fiber>();
            DA.GetDataList<Fiber>("Fiber set 3", FiberSet3);
            List<Fiber> FiberSet4 = new List<Fiber>();
            DA.GetDataList<Fiber>("Fiber set 4", FiberSet4);

            List<Fiber> SortedFibersBigList = FiberSet1
                .Concat(FiberSet2)
                .Concat(FiberSet3)
                .Concat(FiberSet4)
                .ToList();

            for (int i = 0; i < SortedFibersBigList.Count; i++) // change fiber sorting ID
            {
                SortedFibersBigList[i].FiberSortingIndex = i;
            }

            //double tolerance = 0.0; 
            //DA.GetData<double>("Order Tolerance", ref tolerance);
            double pinCapacity = 0.0;
            DA.GetData<double>("Pin Capacity", ref pinCapacity);
            //double duplicateMax = 0.0;
            //DA.GetData<double>("Duplicate Maximum", ref duplicateMax);

            double fiberAmount = 0.0;
            DA.GetData<double>("Fiber Amount", ref fiberAmount);

            double skipRatio = SortedFibersBigList.Count / fiberAmount;
            int skipRatioInt = (int)skipRatio;

            List<Fiber> SortedFibers = new List<Fiber>();

            if (fiberAmount != 0)
            {
                for (int i = 0; i < SortedFibersBigList.Count; i++)
                {
                    if (i % skipRatioInt == 0)
                        SortedFibers.Add(SortedFibersBigList[i]);
                }
            }

            else
            {
                SortedFibers = SortedFibersBigList;
            }


            // component output
            List<Fiber> FiberSyntax = new List<Fiber>();
            List<string> PinIDsNote = new List<string>();

            // initialize
            PathAgent PathAgent = new PathAgent(SortedFibers[0].PinA, SortedFibers[0], SortedFibers, pinCapacity); // the first pin is pinA of the first fiber on the list 
            FiberSyntax.Add(SortedFibers[0]);
            SortedFibers[0].Direction = "AB";
            SortedFibers[0].StartPinID = "A";
            SortedFibers[0].EndPinID = "B";

            PathAgent.MarkDownCurrentPin();

            // add connected pins for first fiber, for duplicate check
            PathAgent.CurrentPin.ConnectedPins.Add(SortedFibers[0].PinB);
            SortedFibers[0].PinB.ConnectedPins.Add(SortedFibers[0].PinA);

            // mark down first fab index
            PathAgent.FiberFabricationIndexNote.Add(SortedFibers[0].FiberSortingIndex);

            // Loop of searching fibers begin
            for (int i = 0; i < SortedFibers.Count; i++)
            {

                PathAgent.GoToTheOtherPin(); // currentPin is changed according to current fiber
                PathAgent.MarkDownCurrentPin();
                // updated fiber

                // !!!
                PathAgent.SearchFiber(); // currentFiber & currentPin(may go to its neighbours) are changed 
                // !!!
                if (PathAgent.CurrentPin.FrameID == "A")
                {
                    PathAgent.CurrentFiber.Direction = "AB";
                    PathAgent.CurrentFiber.StartPinID = PathAgent.CurrentFiber.PinA.PinID;
                    PathAgent.CurrentFiber.EndPinID = PathAgent.CurrentFiber.PinB.PinID;

                    //PathAgent.CurrentFiber.StartPin = PathAgent.CurrentFiber.PinA;
                    //PathAgent.CurrentFiber.EndPin = PathAgent.CurrentFiber.PinB;

                }

                else
                {
                    PathAgent.CurrentFiber.Direction = "BA";
                    PathAgent.CurrentFiber.StartPinID = PathAgent.CurrentFiber.PinB.PinID;
                    PathAgent.CurrentFiber.EndPinID = PathAgent.CurrentFiber.PinA.PinID;

                    //PathAgent.CurrentFiber.StartPin = PathAgent.CurrentFiber.PinB;
                    //PathAgent.CurrentFiber.EndPin = PathAgent.CurrentFiber.PinA;
                }

                PathAgent.MarkDownCurrentPin(); // because the current pin can go to its neighbours, mark down again
                FiberSyntax.Add(PathAgent.CurrentFiber);

                // evaluate
                //if (PathAgent.CurrentFiber.FiberSortingIndex - i > 0) // && intersect with each other
                //    PathAgent.SyntaxEvaluation += PathAgent.CurrentFiber.FiberSortingIndex - i;

            }
            List<Curve> ContinousFiberCrv = new List<Curve>();
            ContinousFiberCrv = FiberSyntax.Select(o => o.FiberCrv).ToList();
            List<double> FiberFabricationIndexs = PathAgent.FiberFabricationIndexNote;

            // Outputing
            PinIDsNote = PathAgent.PinIDsNote;

            DA.SetDataList("Custom Fibers", FiberSyntax);

            DA.SetDataList("Syntax Note", PinIDsNote);

            DA.SetDataList("Fiber Curves", ContinousFiberCrv);

            DA.SetDataList("Fiber Sorting Indexes", FiberFabricationIndexs);

            //DA.SetData("Syntax Evaluation", PathAgent.SyntaxEvaluation);

            DA.SetDataList("Skipped Fiber Curves", PathAgent.SkippedFibers.Select(o => o.FiberCrv).ToList());

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
            get { return new Guid("{3c05d3d3-cfb1-4be7-90fe-c720063d1780}"); }
        }
    }
}
