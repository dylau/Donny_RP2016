using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using RP2k1516;

namespace RP1516
{
    public class Frame
    {
        
        public string ID = null; // A or B
        public List<Pin> PinsOnFrame = new List<Pin>();
        public Curve FrameCurve = null;

        public Frame(Curve crv, int count, string id)
        {
            ID = id;
            if (crv.PointAtStart.Y < crv.PointAtEnd.Y) // check direction: start point of the frame is with a smaller y.
            {
                FrameCurve = crv;
            }
            else
            {
                crv.Reverse();
                FrameCurve = crv;
            }

            for (int i = 0; i < count; i++)
            {
                double t = Convert.ToDouble(i) / Convert.ToDouble(count-1) ;
                Point3d p = FrameCurve.PointAt(t);
                Pin pin = new Pin(p, FrameCurve, ID, i);
                PinsOnFrame.Add(pin);
                
            }
        }
    }
}
