using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GH_IO.Types;
using Rhino.Geometry;
using Grasshopper;


namespace RP2k1516
{
    public class LandingPlatform
    {
        // landing platform is named: H_1, H_2, etc.
        public string Name = null;
        public List<GH_Point3D> Position = new List<GH_Point3D>();
    }
}
