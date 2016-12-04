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
        public Boolean FiberSerachingDone; 

        public Pin CurrentPin; 
        public List<Fiber> VisibleFibers; // fiber list for searching 
        public List<Fiber> SkippedFibers; // disgard the remote fibers in the sorted list
        public Fiber CurrentFiber; // the fiber whose end pin is the current pin
        public List<double> FiberFabricationIndexNote; // indicate the fabrication order of the sorted fibers, used to check how the syntax is defferent from sorted order
        public double Tolerance;
        public double SyntaxEvaluation; // evaluate how the path selection is contradictory to the curvature
        public double DuplicateMax; // the maximum amount of fiber a connection can repeat
        public List<Fiber> SortedFibers;
        public double RemoveEdge;
        public double PinCapacity;

        public PathAgent(Pin initialPin, Fiber initialFiber, List<Fiber> sortedFibers, double pinCapacity)
        {
            CurrentPin = initialPin;
            CurrentFiber = initialFiber;
            PinIDsNote = new List<string>();
            FiberFabricationIndexNote = new List<double>();
            SyntaxEvaluation = 0.0;
            FiberSerachingDone = false;
            SkippedFibers = new List<Fiber>();
            SortedFibers = sortedFibers;
            PinCapacity = pinCapacity;
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

                if (CurrentPin.FrameID == "A")
                {
                    CurrentPin = iFiber.PinA; // update current pin to its neighbour/itself on same frame
                    // constrains
                    // 1. skip fiber already laid
                    List<int> connectedPinIDs = CurrentPin.ConnectedPins.Select(o => o.Index).ToList(); // all already connected pins' ID
                    if (connectedPinIDs.Exists(o => o == iFiber.PinB.Index)) // the nearest fiber is alrealdy laid, go to next near fiber, no duplication is allowed
                        ;
                    // 4. out of pin capacity
                    //if (CurrentFiber.PinB.ConnectedPins.Count > PinCapacity)
                    //    SkippedFibers.Add(iFiber);
                    // 2. do not allow too much set back
                    //if (CurrentFiber.FiberSortingIndex - iFiber.FiberSortingIndex > Tolerance) // skip fibers for better curvature
                    //    SkippedFibers.Add(iFiber);
                    // 3.edge condition: both too small or both to big, skip
                    //else if ((CurrentPin.Index > (60 - RemoveEdge) && iFiber.PinB.Index > (26 - RemoveEdge)) || (CurrentPin.Index < RemoveEdge && iFiber.PinB.Index < RemoveEdge))
                    //    SkippedFibers.Add(iFiber);
                    //if (CurrentPin.Position.Z < 1 && iFiber.PinB.Position.Z < 1)
                    //SkippedFibers.Add(iFiber);

                    // all constrains are saticfied
                    else
                    {
                        //Fiber ifiber = VisableFibers.OrderByDescending(o => o.FiberSortingID).ToList()[VisableFibers.Count - 1]; // !!!
                        CurrentFiber = iFiber;
                        //iFiber.FiberFabricationIndex = i;
                        FiberFabricationIndexNote.Add(iFiber.FiberSortingIndex);
                        // Add connection info !! 
                        CurrentPin.ConnectedPins.Add(CurrentFiber.PinB);
                        CurrentFiber.PinB.ConnectedPins.Add(CurrentPin);
                        break;
                    }
                }

                if (CurrentPin.FrameID == "B")
                {
                    CurrentPin = iFiber.PinB; 
                    // constrains
                    // 1. skip fiber already laid
                    List<int> connectedPinIDs = CurrentPin.ConnectedPins.Select(o => o.Index).ToList();
                    if (connectedPinIDs.Exists(o => o == iFiber.PinA.Index)) // the nearest fiber is alrealdy laid, go to next near fiber, no duplication is allowed
                        ;
                    // 4. out of pin capacity
                    //if (CurrentFiber.PinA.ConnectedPins.Count > PinCapacity)
                    //    SkippedFibers.Add(iFiber);
                    // 2. do not allow too much set back
                    //if (CurrentFiber.FiberSortingIndex - iFiber.FiberSortingIndex > Tolerance) // skip fibers for better curvature
                    //    SkippedFibers.Add(iFiber);
                    // 3.edge condition: both too small or both to big, skip
                    //else if ((CurrentPin.Index > (26 - RemoveEdge) && iFiber.PinB.Index > (60 - RemoveEdge)) || (CurrentPin.Index < RemoveEdge && iFiber.PinA.Index < RemoveEdge))
                    //    SkippedFibers.Add(iFiber);
                    //if (CurrentPin.Position.Z < 1 && iFiber.PinB.Position.Z < 1)
                    //SkippedFibers.Add(iFiber);

                    // all constrains are saticfied
                    else
                    {
                        CurrentFiber = iFiber;
                        //iFiber.FiberFabricationIndex = i;
                        FiberFabricationIndexNote.Add(iFiber.FiberSortingIndex);

                        // Add connection info
                        CurrentPin.ConnectedPins.Add(CurrentFiber.PinA);
                        CurrentFiber.PinA.ConnectedPins.Add(CurrentPin);
                        break;
                    }
                }
            }
        }
    }
}
