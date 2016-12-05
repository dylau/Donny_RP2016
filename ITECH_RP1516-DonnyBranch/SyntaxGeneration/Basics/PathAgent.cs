using System;
using System.Collections.Generic;
using System.Linq;

namespace RP1516
{
    public class PathAgent
    {
        public double NeighbourRange; // neighbour range
        public List<Pin> NeighbourPins; // including the pin itself, e.g. range = 2, 5 neighbours 
        public List<string> PinIDsNote; // for output
        public List<string> FiberIDsNote; // for output

        public Pin CurrentPin; 
        public List<Fiber> VisibleFibers; // fiber list for searching 
        public List<Fiber> SkippedFibers; // disgard the remote fibers in the sorted list
        public Fiber CurrentFiber; // the fiber whose end pin is the current pin
        public List<double> FiberFabricationIndexNote; // indicate the fabrication order of the sorted fibers, used to check how the syntax is defferent from sorted order
        public double Tolerance;
        public double SyntaxEvaluation; // evaluate how the path selection is contradictory to the curvature
        public double DuplicateMax = 1; // the maximum amount of fiber a connection can repeat
        public List<Fiber> SortedFibers;
        public double RemoveEdge;
        public double PinCapacity;

        public bool FiberSearchFlag = true; 

        public PathAgent(Pin initialPin, Fiber initialFiber, List<Fiber> sortedFibers)
        {
            CurrentPin = initialPin;
            CurrentFiber = initialFiber;
            PinIDsNote = new List<string>();
            FiberFabricationIndexNote = new List<double>();
            SyntaxEvaluation = 0.0;
            SkippedFibers = new List<Fiber>();
            SortedFibers = sortedFibers;
        }

        public void GoToTheOtherPin()
        {
            if (CurrentPin.FrameID == "A")
                CurrentPin = CurrentFiber.PinB;
            else
                CurrentPin = CurrentFiber.PinA;
        }

        public void MarkDownCurrentPin()
        {
            PinIDsNote.Add(CurrentPin.PinID);
        }

        public void SearchFiber()
        {
            // group visable fibers from the current pin and its neighbour pins
            List<Fiber> VisibleNeighbourFibers = new List<Fiber>();
            CurrentPin.NeighbourPins.ForEach(o => VisibleNeighbourFibers.AddRange(o.VisibleFibers)); // group all visible fibers of the neighbour pins
            VisibleFibers = CurrentPin.VisibleFibers.Concat(VisibleNeighbourFibers).ToList(); // include the visible fibers of itself
            VisibleFibers.Remove(CurrentFiber); // remove the just laid fiber out of the visible fiber list
            VisibleFibers.RemoveAll(o => !SortedFibers.Contains(o));

            // sort from good to bad
            List<Fiber> SortedVisableFibers = VisibleFibers.OrderBy(o => o.FiberSortingIndex).ToList(); // !!! very important

            for (int i = 0; i < SortedVisableFibers.Count; i++) // loop from good to bad in the fiber candidates, once all constrains satisfied, search done, next fiber decided.
            {
                Fiber iFiber = SortedVisableFibers[i];

                if (iFiber.LaidCount > DuplicateMax) // skip duplicate 
                {
                    ;
                    if (i == SortedVisableFibers.Count - 1) // already last visible fiber
                        FiberSearchFlag = false;
                    else
                        ;
                }

                else
                {
                    if (CurrentPin.FrameID == "A")
                    {
                        CurrentPin = iFiber.PinA; // update current pin to its neighbour/itself on same frame

                        FiberSearchFlag = true;

                        CurrentFiber = iFiber;
                        FiberFabricationIndexNote.Add(iFiber.FiberSortingIndex);

                        iFiber.LaidCount += 1; // Add connection info
                        iFiber.PinA.ConnectedPins.Add(iFiber.PinB);
                        iFiber.PinB.ConnectedPins.Add(CurrentPin);
                        break;
                        //}
                    }

                    if (CurrentPin.FrameID == "B")
                    {
                        CurrentPin = iFiber.PinB;
                        // constrains
                        // 1. skip fiber already laid
                        //List<int> connectedPinIDs = CurrentPin.ConnectedPins.Select(o => o.Index).ToList();
                        //if (connectedPinIDs.Exists(o => o == iFiber.PinA.Index)) // the nearest fiber is alrealdy laid, go to next near fiber, no duplication is allowed
                        //    FiberSearchFlag = false;

                        // all constrains are saticfied
                        //else
                        //{
                        FiberSearchFlag = true;

                        CurrentFiber = iFiber;
                        FiberFabricationIndexNote.Add(iFiber.FiberSortingIndex); //!!!

                        iFiber.LaidCount += 1; // Add connection info
                        iFiber.PinB.ConnectedPins.Add(iFiber.PinA);
                        iFiber.PinA.ConnectedPins.Add(CurrentPin);
                        break;
                        //}
                    }
                }
            }
        }
    }
}
