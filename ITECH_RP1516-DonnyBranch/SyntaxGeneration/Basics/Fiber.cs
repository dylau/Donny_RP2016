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
        public Curve FiberCrv; 
        public int FiberSortingIndex; // sorting order
        public int FiberFabricationIndex; // fabrication order
        public int Day; 
        public Pins AllPins; // if the pins' ID are predefined, this field is unnecessary. 
        public string Type = null; // chain, double curve or arch

        public string StartPinID = null; // e.g. A000, A001
        public string EndPinID = null; // e.g. A000, A001
        public Pin PinA = null;
        public Pin PinB = null;

        public string Direction; // AB or BA 
        public string Material; // Glass or Carbon

        public List<TurningPoint> TurningPts;
        public Line StraightLine;
        public List<Point3d> TestPts;

        // keys for sorting 
        public double LengthDifference; 
        public double Potential; 
        public double PinPotential; 
        public double MeanDistance;
        public double MaximumDistance; // absolute value
        public double IntegratedScore; // for nonchains
        public double Curliness;
        
        // relative height
        public List<Fiber> RelatedFibers = new List<Fiber>(); // the fibers which are higher should be laid first
        public List<double> Distances = new List<double>(); // the distances to its related lines
        public List<bool> IsHigher = new List<bool>(); // the high/low relations to its related lines

        // structure parameter
        public double StructureFactor = 0.0;

        // instantiation
        public Fiber(Curve fiberCrv, int fiberID, string direction, string material)
        {
            FiberCrv = fiberCrv;
            Material = material;
            FiberSortingIndex = fiberID;
            Direction = direction;
            DetermineChain();
            CalculateCurliness();

        }

        public void CalculatePinPotential() // bigger, higher, lay earlier
        {
            double pinPotential;
            pinPotential = FiberCrv.PointAtEnd.Z + FiberCrv.PointAtStart.Z;
            PinPotential = pinPotential;
        }

        public void DetermineChain()
        {
            Line straightLine = new Line(FiberCrv.PointAtStart, FiberCrv.PointAtEnd);
            StraightLine = straightLine; // generate a straightline

            /*
            double distance = StraightLine.PointAt(0.5).Z - FiberCrv.PointAt(0.5).Z;

            if (distance < 0)
                Type = "Positive and Both";
            else
                Type = "Negative";

            MaximumDistance = Math.Abs(distance);
            */
            #region // complex way
            
            bool flag = true;
            List<double> distances = new List<double>(); // verticle distance, +- 

            List<Point3d> PtsCrv = new List<Point3d>();
            List<Point3d> PtsLine = new List<Point3d>();

            int segment = 20; 
            PtsCrv = Utils.PublicatePointsOnCurve(FiberCrv, segment);
            PtsLine = Utils.PublicatePointsOnCurve(StraightLine.ToNurbsCurve(), segment);

            for (int i = 0; i < segment; i++) // calculate the distance between crv and staightline
            {

                double d = PtsCrv[i].Z - PtsLine[i].Z; // negative for a negative fiber 
                distances.Add(d);

                if (d <= 0)
                    ;
                else
                {
                    flag = false;
                }
            }
            MeanDistance = distances.Sum() / (double)segment; // +-
            MaximumDistance = - distances.Min(); // +
            if (flag)
                Type = "Negative";
            else
                Type = "Positive and Both";
            
            #endregion
        }

        public void CalculateCurliness()
        {
            Line straightLine = new Line(FiberCrv.PointAtStart, FiberCrv.PointAtEnd);
            StraightLine = straightLine; // generate a straightline
            
            int segment = 10;
            List<Point3d> PtsCrv = new List<Point3d>();
            List<Point3d> PtsLine = new List<Point3d>();
            PtsCrv = Utils.PublicatePointsOnCurve(FiberCrv, segment);
            PtsLine = Utils.PublicatePointsOnCurve(StraightLine.ToNurbsCurve(), segment);

            for (int i = 0; i < segment; i++) // calculate the distance between crv and staightline
            {
                double d = Math.Sqrt(PtsCrv[i].DistanceTo(PtsLine[i])); // negative
                Curliness += d;
            }

        }

        public void StrutureValue(List<SoFiNode> allNodes)
        {
            // using compression
            double value = 0.0;
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

           // extra sorting methods
           /*
           public void CalculateLengthDifference()
           {
               Line straightLine = new Line(FiberCrv.PointAtStart, FiberCrv.PointAtEnd);
               double lengthDifference = FiberCrv.GetLength() - straightLine.Length;
               LengthDifference = lengthDifference;
           }

           public void CalculatePotential()
           {
               double potential = 0.0;
               double t;
               for (int i = 0; i < 20; i++)
               {
                   t = Convert.ToDouble(i) / Convert.ToDouble(20 - 1);
                   potential += FiberCrv.PointAt(t).Z;
               }
               Potential = potential;
           }
           */
           /*
           public void FindTuringPtOnCrv()
           {
               List<TurningPoint> turningPts = Utils.FindTurningPt(FiberCrv);
               if (turningPts.Count == 1 && turningPts[0].Type == "max")
               {
                   Type = "Arch";
               }

               if (turningPts.Count == 1 && turningPts[0].Type == "min")
               {
                   Type = "Chain";
               }

               if (turningPts.Count > 1 || turningPts.Count == 0)
               {
                   Type = "Doubly";
               }
               TurningPts = turningPts;

           } // not working
           */
           /*
           public void DetermineRelativeHeight(Fiber theOtherFiber)
           {
               double a, b;
               bool boo = false;
               boo  = Intersection.LineLine(this.StraightLine, theOtherFiber.StraightLine, out a, out b);
               if (boo) // they intersect!
               {
                   Point3d intersectPointOnThisLine = new Point3d(this.StraightLine.PointAt(a));
                   Point3d intersectPointOnTheOtherLine = new Point3d(theOtherFiber.StraightLine.PointAt(b));
                   double distance = intersectPointOnThisLine.DistanceTo(intersectPointOnTheOtherLine);
                   // collect info for this fiber
                   RelatedFibers.Add(theOtherFiber);
                   Distances.Add(distance);
                   IsHigher.Add(intersectPointOnThisLine.Z > intersectPointOnTheOtherLine.Z);
                   // collect info for the other fiber
                   theOtherFiber.RelatedFibers.Add(this);
                   theOtherFiber.Distances.Add(distance);
                   theOtherFiber.IsHigher.Add(intersectPointOnThisLine.Z < intersectPointOnTheOtherLine.Z);
               }

           }
           */




    }

}





