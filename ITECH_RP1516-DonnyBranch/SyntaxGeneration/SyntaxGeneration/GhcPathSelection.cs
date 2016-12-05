﻿using System;
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
            pManager.AddNumberParameter("Max Daily Fiber Count", "Max Daily Fiber Count", "Max Daily Fiber Count", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)   
        {
            pManager.AddGenericParameter("Custom Fibers", "Custom Fibers", "Custom Fibers", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fiber Curves", "Fiber Curves", "Fiber Curves", GH_ParamAccess.list);
            pManager.AddTextParameter("Syntax Note", "Syntax Note", "Syntax Note", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fiber Sorting Indexes", "Fiber Sorting Indexes", "Fiber Sorting Indexes", GH_ParamAccess.list);
            pManager.AddNumberParameter("Missed Fiber Count", "Missed Fiber Count", "Missed Fiber Count", GH_ParamAccess.item);
        }
            
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Fiber> SortedFibers = new List<Fiber>();
            DA.GetDataList<Fiber>("Sorted Custom Fibers", SortedFibers);
            SortedFibers.ForEach(o => o.LaidCount = 0);

            double maxDaily = 200;
            DA.GetData<double>("Max Daily Fiber Count", ref maxDaily);

            int missedFiberCount = 0;

            // component output
            List<Fiber> FiberSyntax = new List<Fiber>();
            List<string> PinIDsNote = new List<string>();

            // initialize
            //FiberSyntax.Add(SortedFibers.ElementAt(0));
            SortedFibers.ElementAt(0).Direction = "AB";
            SortedFibers.ElementAt(0).StartPinID = "A";
            SortedFibers.ElementAt(0).EndPinID = "B";

            PathAgent PathAgent = new PathAgent(SortedFibers.ElementAt(0).PinA, SortedFibers.ElementAt(0), SortedFibers); // the first pin is pinA of the first fiber on the list 

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

                    }

                    else
                    {
                        PathAgent.CurrentFiber.Direction = "BA";
                        PathAgent.CurrentFiber.StartPinID = PathAgent.CurrentFiber.PinB.PinID;
                        PathAgent.CurrentFiber.EndPinID = PathAgent.CurrentFiber.PinA.PinID;

                    }
                    PathAgent.MarkDownCurrentPin(); // because the current pin can go to its neighbours, mark down again
                    FiberSyntax.Add(PathAgent.CurrentFiber);
                }

                else // no suitable path is possible, need to cut and start again
                {
                    missedFiberCount = SortedFibers.Count - i;
                    break;
                }
               

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

            DA.SetData("Missed Fiber Count", missedFiberCount);
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
