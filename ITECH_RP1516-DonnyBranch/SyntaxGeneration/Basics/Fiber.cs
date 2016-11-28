using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using RP2k1516;
using Rhino.Geometry.Intersect;

namespace RP1516
{
    public class Fiber
    {
        // geo info
        public Curve FiberCrv;
        public Pin PinA = null;
        public Pin PinB = null;
        public Line StraightLine;
        public List<Point3d> PtsOnCurve;

        // basic info
        public string StartPinID = null; // e.g. A000, A001
        public string EndPinID = null; // e.g. A000, A001
        public string Direction; // AB or BA 

        // sorting info
        public int FiberSortingIndex; // sorting order
        public int FiberFabricationIndex; // fabrication order
        public string Type = null; // Positive, Negative, Mixed

        // sorting keys 
        public double IntegratedScore; // for nonchains
        public double Curliness = 0;
        public double MaximumDistance; // absolute value
        public double MeanDistance;

        // structure parameter
        public double StructureFactor = 0.0; // bigger factor shows more importance

        // instantiation
        public Fiber(Curve fiberCrv, int fiberID, string direction)
        {
            FiberCrv = fiberCrv.Rebuild(5,5,true);
            FiberSortingIndex = fiberID;
            Direction = direction;

            //StraightLine = new Line(FiberCrv.PointAtStart, FiberCrv.PointAtEnd);

            DetermineType(); // P or N?
            CalculateCurliness(); 
        }

        public void DetermineType() 
        {
            int segment = 5;

            bool PositiveFlag = true; // if true, positive
            bool NegativeFlag = true; // if true, negative

            //for (int i = 0; i < segment; i++)
            //{
            //    Vector3d v =  FiberCrv.CurvatureAt((double)i / (double)segment);

            //    if (v.Z < 0) 
            //        NegativeFlag = false;
            //    if (v.Z > 0)
            //        PositiveFlag = false;
            //}

            //if (PositiveFlag)
            //    Type = "Postitive";
            //if (NegativeFlag)
            //    Type = "Negative";
            //if (!PositiveFlag && !NegativeFlag)
            //    Type = "Mixed";

            if (FiberCrv.CurvatureAt(0.5).Z > 0) // N
                Type = "Negative";
            if (FiberCrv.CurvatureAt(0.5).Z < 0) // N
                Type = "Positive";



            //List<double> distances = new List<double>(); // verticle distance, +- 

            //List<Point3d> PtsOnCrv = new List<Point3d>();
            //List<Point3d> PtsOnLine = new List<Point3d>();

            //PtsOnCrv = Utils.PublicatePointsOnCurve(FiberCrv, segment);
            //PtsOnLine = Utils.PublicatePointsOnCurve(StraightLine.ToNurbsCurve(), segment);

            //for (int i = 0; i < segment; i++) // calculate the distance between crv and staightline
            //{
            //    double d = PtsOnCrv[i].Z - PtsOnLine[i].Z; // negative for a negative fiber 
            //    distances.Add(d);


            //}

            //MeanDistance = distances.Sum() / (double)segment; // +-
            //MaximumDistance = - distances.Min(); // +

            //if (flag)
            //    Type = "Negative";
            //else
            //    Type = "Positive and Both";

        }

        public void CalculateCurliness()
        {
            Line straightLine = new Line(FiberCrv.PointAtStart, FiberCrv.PointAtEnd);
            StraightLine = straightLine; 

            // generate pts on curve and straight line
            int segment = 10;
            List<Point3d> PtsCrv = new List<Point3d>();
            List<Point3d> PtsLine = new List<Point3d>();
            PtsCrv = Utils.PublicatePointsOnCurve(FiberCrv, segment);
            PtsLine = Utils.PublicatePointsOnCurve(StraightLine.ToNurbsCurve(), segment);
            PtsOnCurve = PtsCrv;
            Curliness = 0;
            for (int i = 0; i < segment; i++) // calculate the distance between crv and staightline
            {

                double d = Math.Sqrt( PtsCrv[i].DistanceTo(PtsLine[i]) * PtsCrv[i].DistanceTo(PtsLine[i])); 
                Curliness += d;
            }

        }

        public void StrutureValue(List<SoFiNode> allNodes)
        {
            // using compression
            List<SoFiNode> PassingSofiNodes = new List<SoFiNode>();

            foreach (SoFiNode inode in allNodes)
            {
                if (Utils.DistancePointToCurve(inode.Node, FiberCrv) < 0.01)
                {
                    PassingSofiNodes.Add(inode);
                }
            }
            PassingSofiNodes.ForEach(o => StructureFactor += Math.Abs(o.StressCom));
        }  // heavy!
    }

}





