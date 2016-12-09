using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using RP2k1516;


namespace RP1516
{
    public class Pin
    {
        public int Index = -1;  // 01
        public string FrameID = null; // A/B
        public string PinID = null; //A01
        public Curve FrameCrv;
        public List<Fiber> VisibleFibers; // potential fibers start with this pin
        public Point3d Position;
        public List<Pin> ConnectedPins; // connected with fiebrs
        public List<int> ConnectedPinsCount; // how many fibers for each connection
        public List<Pin> NeighbourPins; // on the same frame, defined by neighbour range
        public Pin SelectedNeighbourPin;
        public int PinLoad = 0;

        public Pin(Point3d position, Curve frameCrv, string frameID, int index)
        {
            Position = position;
            frameCrv = FrameCrv;
            FrameID = frameID;
            Index = index;
            PinID = String.Format("{0}{1}", FrameID, Index);
            VisibleFibers = new List<Fiber>();
            ConnectedPins = new List<Pin>();
            NeighbourPins = new List<Pin>();
            ConnectedPinsCount = new List<int>();
            SelectedNeighbourPin = this; // defaul setting the neighbour is itself
        }
    }
}
