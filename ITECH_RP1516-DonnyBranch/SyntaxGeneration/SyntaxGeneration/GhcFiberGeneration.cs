using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using RP2k1516;

namespace RP1516
{
    public class GhcGeodesic : GH_Component
    {
        public GhcGeodesic()
          : base("FiberGeneration", "FiberGeneration",
              "FiberGeneration",
              "RP1516", "SyntaxGeneration")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Guide Surface", "Guide Surface", "Guide Surface", GH_ParamAccess.item);
            pManager.AddCurveParameter("Frame A", "Frame A", "Frame A", GH_ParamAccess.item);
            pManager.AddPointParameter("Pins on Frame A", "Pins on Frame A", "Pins on Frame A", GH_ParamAccess.list);
            pManager.AddCurveParameter("Frame B", "Frame B", "Frame B", GH_ParamAccess.item);
            pManager.AddPointParameter("Pins on Frame B", "Pins on Frame B", "Pins on Frame B", GH_ParamAccess.list);
            pManager.AddNumberParameter("Neighbour Range", "Neighbour Range", "Neighbour Range", GH_ParamAccess.item); // search for adequate fibers within the neighbours of one pin to generate better curvature
            pManager.AddTextParameter("Fiber Generation Type", "Fiber Generation Type", "Fiber Generation Type", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("All Fibers", "All Fibers", "All Fibers", GH_ParamAccess.list);
            pManager.AddCurveParameter("All Fiber Curves", "All Fiber Curves", "All Fiber Curves", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // -------------------------------------------- Generating Fibers --------------------------------------------
            Surface GuideSrf = null;
            DA.GetData<Surface>("Guide Surface", ref GuideSrf);
            Curve CrvA = null;
            Curve CrvB = null;
            DA.GetData<Curve>("Frame A", ref CrvA);
            DA.GetData<Curve>("Frame B", ref CrvB);
 
            List<Point3d> pointsA = new List<Point3d>();
            List<Point3d> pointsB = new List<Point3d>();
            DA.GetDataList<Point3d>("Pins on Frame A", pointsA);
            DA.GetDataList<Point3d>("Pins on Frame B", pointsB);
            //sort points according to y coordinate before use them to initialize pins
            pointsA.OrderBy(o => o.Y).ToList();
            pointsB.OrderBy(o => o.Y).ToList();

            double neighbourRange = 0.0;
            DA.GetData<double>("Neighbour Range", ref neighbourRange);

            string fiberGenerationType = null;
            DA.GetData<string>("Fiber Generation Type", ref fiberGenerationType);

            // instantiate Frame
            double PinCountA = pointsA.Count;
            double PinCountB = pointsB.Count;
            Frame FrameA = new Frame(CrvA, Convert.ToInt32(PinCountA), "A");
            Frame FrameB = new Frame(CrvB, Convert.ToInt32(PinCountB), "B");

            // instantiate Pin
            List<Pin> PinsA = new List<Pin>();
            List<Pin> PinsB = new List<Pin>();

            // insdanciate the pins with neighbour definition 
            for (int i = 0; i <= (Int16)neighbourRange; i++)  
            {
                Pin iPin = new Pin(pointsA[i], FrameA.FrameCurve, "A", i);
                PinsA.Add(iPin);
                //define neighbouring pins
                iPin.NeighbourPins.AddRange(PinsA);
                PinsA.ForEach(o => o.NeighbourPins.Add(iPin));
            }

            for (int i = (Int16)neighbourRange + 1; i < pointsA.Count; i++) 
            {
                Pin iPin = new Pin(pointsA[i], FrameA.FrameCurve, "A", i);
                PinsA.Add(iPin);
                //define neighbouring pins
                iPin.NeighbourPins.AddRange(PinsA.GetRange(i - (Int16)neighbourRange, (Int16)neighbourRange)); // iPin define its neighbours on its left 
                PinsA.GetRange(i - (Int16)neighbourRange, (Int16)neighbourRange).ForEach(o => o.NeighbourPins.Add(iPin)); // iPin's neighbours define iPin as their neighbours(iPin is the latest initialized Pin, so its on their right)
            }

            for (int i = 0; i <= (Int16)neighbourRange; i++) 
            {
                Pin iPin = new Pin(pointsB[i], FrameB.FrameCurve, "B", i);
                PinsB.Add(iPin);
                //define neighbouring pins
                iPin.NeighbourPins.AddRange(PinsB);
                PinsB.ForEach(o => o.NeighbourPins.Add(iPin));
            }

            for (int i = (Int16)neighbourRange + 1; i < pointsB.Count; i++) 
            {
                Pin iPin = new Pin(pointsB[i], FrameB.FrameCurve, "B", i);
                PinsB.Add(iPin);
                //define neighbouring pins
                iPin.NeighbourPins.AddRange(PinsB.GetRange(i - (Int16)neighbourRange, (Int16)neighbourRange));
                PinsB.GetRange(i - (Int16)neighbourRange, (Int16)neighbourRange).ForEach(o => o.NeighbourPins.Add(iPin));
            }
            
            FrameA.PinsOnFrame = PinsA;
            FrameB.PinsOnFrame = PinsB;

            List<Fiber> AllPossibleFibers = Utils.GenerateFibers(PinsA, PinsB, GuideSrf, fiberGenerationType); // all crvs now are from A-B, startPin = Ax, endPin = Bx
            // known properties:
            // fiberCvr;
            // fiberID = -1;
            // direction = "AB";
            // material = "MAT";
            // pinA;
            // pinB;

            DA.SetDataList("All Fibers", AllPossibleFibers);

            List<Curve> AllFiberCurves = new List<Curve>();
            AllFiberCurves = AllPossibleFibers.Select(o => o.FiberCrv).ToList();
            DA.SetDataList("All Fiber Curves", AllFiberCurves);

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
            get { return new Guid("{72ff915c-65ac-4c4a-a24b-568e4c0d2686}"); }
        }
    }
}