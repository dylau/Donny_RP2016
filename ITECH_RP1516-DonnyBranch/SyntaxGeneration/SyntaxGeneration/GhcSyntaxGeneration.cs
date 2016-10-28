using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using RP2k1516;

    
namespace RP1516
{
    public class GhcSyntaxGeneration : GH_Component
    {

        public GhcSyntaxGeneration()
          : base("SyntaxGeneration", "SyntaxGeneration",
              "SyntaxGeneration",
              "RP1516", "SyntaxGeneration")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Custom Sorted Fibers", "Custom Sorted Fibers", "Custom Sorted Fibers", GH_ParamAccess.list);
            //pManager.AddNumberParameter("Fiber Count", "Fiber Count", "Fiber Count", GH_ParamAccess.item);
            //pManager.AddNumberParameter("Pin Capa")

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Custom Fibers", "Custom Fibers", "Custom Fibers", GH_ParamAccess.list);
            pManager.AddCurveParameter("Fiber Curves", "Fiber Curves", "Fiber Curves", GH_ParamAccess.list);
            pManager.AddTextParameter("Syntax Note", "Syntax Note", "Syntax Note", GH_ParamAccess.list);
            pManager.AddNumberParameter("Fiber Fabrication Indexes", "Fiber Fabrication Indexes", "Fiber Fabrication Indexes", GH_ParamAccess.list);

        }
 
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Fiber> SortedFibers = new List<Fiber>();
            DA.GetDataList<Fiber>("Custom Sorted Fibers", SortedFibers);

            //double fiberCount = 0.0;
            //DA.GetData<double>("Fiber Count", ref fiberCount);

            List<Fiber> ContinuousFibers = new List<Fiber>();
            List<string> PinIDsNote = new List<string>();

            //initialize
            PathAgent PathAgent = new PathAgent(SortedFibers[0].PinA, SortedFibers[0], 0.0);
            ContinuousFibers.Add(SortedFibers[0]);
            PathAgent.MarkDownCurrentPin();

            for (int i = 0; i < SortedFibers.Count; i++)
            {
                PathAgent.GoToTheOtherPin(); //currentPin is changed
                PathAgent.MarkDownCurrentPin();
                // updated fiber
                PathAgent.SearchFiber(); //currentFiber & currentPin are changed 
                PathAgent.MarkDownCurrentPin();
                ContinuousFibers.Add(PathAgent.CurrentFiber);
            }

            PinIDsNote = PathAgent.PinIDsNote;

            DA.SetDataList("Custom Fibers", ContinuousFibers);
            DA.SetDataList("Syntax Note", PinIDsNote);

            List<Curve> ContinousFiberCrv = new List<Curve>();
            ContinousFiberCrv = ContinuousFibers.Select(o => o.FiberCrv).ToList();

            DA.SetDataList("Fiber Curves", ContinousFiberCrv);

            List<double> FiberFabricationIndexs = PathAgent.FiberFabricationIndexes;

            DA.SetDataList("Fiber Fabrication Indexes", FiberFabricationIndexs);


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