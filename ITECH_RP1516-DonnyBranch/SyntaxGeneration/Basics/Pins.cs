using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GH_IO.Types;
using Rhino.Geometry;
using Grasshopper;

namespace RP2k1516
{
    public class Pins 
    {
        // This class takes in fiber curves, groups their end points into frameA and frameB.
        // And sort the pins according to their X coordinates.

        private List<Point3d> frameAPinPositions;
        private List<Point3d> frameBPinPositions;
        public static double HalfDistance;
        public List<Point3d> SortedPinsA;
        public List<Point3d> SortedPinsB;

        public Pins(List<Curve> allCrv) //sort pins, 
        {
            frameAPinPositions = new List<Point3d>();
            frameBPinPositions = new List<Point3d>();

            foreach (Curve fiber in allCrv)
            {
                if (fiber.PointAtStart.X < HalfDistance)
                {
                    frameAPinPositions.Add(fiber.PointAtStart);
                }
                else
                    frameBPinPositions.Add(fiber.PointAtStart);

                if (fiber.PointAtEnd.X < HalfDistance)
                {
                    frameAPinPositions.Add(fiber.PointAtEnd);
                }
                else
                    frameBPinPositions.Add(fiber.PointAtEnd);
            }


            var newlistA = frameAPinPositions.OrderBy(p => p.Y).ToList();
            SortedPinsA = newlistA;

            var newlistB = frameBPinPositions.OrderBy(o => o.Y).ToList();
            SortedPinsB = newlistB;

            HalfDistance = CalculateHalfLength(allCrv);

        }


        private double evaluateCrv(Point3d p, Curve crv)
        {
            double eva = 0.0;



            return eva;
        } // ???


        public static double CalculateHalfLength(List<Curve> allCrvs)
        {

            double HalfLength = 0.0;
            foreach (Curve fiber in allCrvs)
            {
                HalfLength = HalfLength + fiber.PointAtStart.X;
                HalfLength = HalfLength + fiber.PointAtEnd.X;
            }
            HalfLength = HalfLength / allCrvs.Count / 2;

            return HalfLength;
        }



    

    }
}