using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RP2k1516;
using Rhino.Geometry;


namespace RP1516
{
    public class TurningPoint
    {
        public Point3d Point;
        public string Type; //max, min
        public double T;

        public TurningPoint(Point3d pt, string tp, double t)
        {
            Point = pt;
            Type = tp;
            T = t;

        }
    }
}
