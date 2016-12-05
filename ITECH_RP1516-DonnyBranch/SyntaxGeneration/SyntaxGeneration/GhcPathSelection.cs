﻿﻿using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;


namespace RP1516
{
    public class GhcPathSelection : GH_Component
    {
        public GhcPathSelection()
          : base("GhcPathSelection", "GhcPathSelection",
              "GhcPathSelection",
              "RP1516", "SyntaxGeneration")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Sorted Custom Fibers", "Sorted Custom Fibers", "Sorted Custom Fibers", GH_ParamAccess.list);
            pManager.AddNumberParameter("Pin Capacity", "Pin Capacity", "Pin Capacity", GH_ParamAccess.item); // maximum pin capacity
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)   
        {
            pManager.AddGenericParameter("Custom Fibers", "Custom Fibers", "Custom Fibers", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fiber Curves", "Fiber Curves", "Fiber Curves", GH_ParamAccess.list);
            pManager.AddTextParameter("Syntax Note", "Syntax Note", "Syntax Note", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fiber Sorting Indexes", "Fiber Sorting Indexes", "Fiber Sorting Indexes", GH_ParamAccess.list);
        }
            
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Fiber> SortedFibers = new List<Fiber>();
            DA.GetDataList<Fiber>("Sorted Custom Fibers", SortedFibers);

            double pinCapacity = 10.0;
            DA.GetData<double>("Pin Capacity", ref pinCapacity);

            // component output
            List<Fiber> FiberSyntax = new List<Fiber>();
            List<string> PinIDsNote = new List<string>();

            // initialize
            PathAgent PathAgent = new PathAgent(SortedFibers.ElementAt(0).PinA, SortedFibers.ElementAt(0), SortedFibers, pinCapacity); // the first pin is pinA of the first fiber on the list 
            FiberSyntax.Add(SortedFibers.ElementAt(0));
            SortedFibers.ElementAt(0).Direction = "AB";
            SortedFibers.ElementAt(0).StartPinID = "A";
            SortedFibers.ElementAt(0).EndPinID = "B";

            PathAgent.MarkDownCurrentPin();

            // add connected pins for first fiber, for duplicate check
            PathAgent.CurrentPin.ConnectedPins.Add(SortedFibers.ElementAt(0).PinB);
            SortedFibers.ElementAt(0).PinB.ConnectedPins.Add(SortedFibers.ElementAt(0).PinA);

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

                if (PathAgent.FiberSearchFlag)
                {
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
                }

                else
                    break;

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
