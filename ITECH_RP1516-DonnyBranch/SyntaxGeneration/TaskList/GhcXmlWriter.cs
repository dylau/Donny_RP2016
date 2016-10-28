using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace RP1516
{
    public class GhcXmlWriter : GH_Component
    {
        // This is the Ghc class.
        public GhcXmlWriter()
          : base("XmlWriter", "XmlWriter",
              "XmlWriter",
              "RP1516", "XmlWriter")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Fiber Syntax", "Fiber Syntax", "Fiber Syntax", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Generate", "Generate", "Generate", GH_ParamAccess.item);
            pManager.AddTextParameter("Path", "Path", "Path", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Fiber> AllCurves = new List<Fiber>();
            DA.GetDataList<Fiber>("Fiber Syntax", AllCurves);

            string WritePath = null;
            DA.GetData<string>("Path", ref WritePath);

            // Fiber class
            int fiberCount = AllCurves.Count;
            List<string> Info = new List<string>();

            List<string> FiberDirection = new List<string>();
            List<string> PinsID = new List<string>();

            // XML
            bool toGenerate = false;
            DA.GetData<bool>("Generate", ref toGenerate);
            if (toGenerate)
            {
                for (int i = 0; i < fiberCount; i++)  // loop of fibers to wrtie xml
                {
                    Fiber iFiber = AllCurves[i];
                    StepXml iWriter = new StepXml(iFiber, i, WritePath);
                    iWriter.Write();
                }
            }
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
            get { return new Guid("{4a8fc332-165a-42e0-82f7-9e28fe779367}"); }
        }
    }
}