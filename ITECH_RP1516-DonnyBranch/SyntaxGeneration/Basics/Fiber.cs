using System;
using System.Collections.Generic;
using System.Linq;
using Rhino.Geometry;

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
        public List<Point3d> PtsOnLine;

        // basic info
        public string StartPinID = null; // e.g. A000, A001
        public string EndPinID = null; // e.g. A000, A001
        public string Direction; // AB or BA 

        // sorting info
        public int FiberSortingIndex; // sorting order
        public int FiberFabricationIndex; // fabrication order
        public string Type = null; // Positive, Negative, Mixed

        // sorting keys 
        public double Curliness = 0;

        // structure parameter
        public double StructureFactor = 0.0; // bigger factor indicates more important fibers 


        public Fiber(Curve fiberCrv, int fiberID, string direction)
        {
            FiberCrv = fiberCrv.Rebuild(5,5,true);
            FiberSortingIndex = fiberID;
            Direction = direction;

            DetermineType(); // P or N?
            CalculateCurliness();


        }

        public void DetermineType() 
        {
            int segment = 6;
            double increment = 1 / (double)segment;

            List<bool> curvatureFlag = new List<bool>();

            for (int i = 1; i < segment; i++) // check (segment - 1) points
            {
                curvatureFlag.Add(FiberCrv.CurvatureAt(increment * i).Z > 0); // if true, Negative
            }

            int tolerance = 1; // allow tolerance points to be different curved 
            if (curvatureFlag.Where(o => o == true).Count() >= segment - 1 - tolerance)
                Type = "Negative";
            else if (curvatureFlag.Where(o => o == false).Count() >= segment - 1 - tolerance)
                Type = "Positive";
            else
                Type = "Mix";

        }

        public void CalculateCurliness()
        {
            Line straightLine = new Line(FiberCrv.PointAtStart, FiberCrv.PointAtEnd);
            StraightLine = straightLine;

            int Segment = 10;
            PtsOnCurve = Utils.PublicatePointsOnCurve(FiberCrv, Segment);
            PtsOnLine = Utils.PublicatePointsOnCurve(StraightLine.ToNurbsCurve(), Segment);

            Curliness = 0;
            for (int i = 0; i < Segment; i++) // calculate the distance between crv and staightline
            {
                double d = Math.Sqrt( PtsOnCurve[i].DistanceTo(PtsOnLine[i]) * PtsOnCurve[i].DistanceTo(PtsOnLine[i]) ); 
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





