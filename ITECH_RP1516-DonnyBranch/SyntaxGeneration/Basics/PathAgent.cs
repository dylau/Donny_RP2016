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

                if (iFiber.LaidCount >= 1) // skip duplicate 
                {
                    ;
                    if (i == SortedVisableFibers.Count - 1) // already last visible fiber
                        FiberSearchFlag = false;
                    else
                        ;
                }

                else // this is the fiber we are looking for!
                {
                    if (CurrentPin.FrameID == "A") // this fiber is from A-B
                    {
                        iFiber.Direction = "AB";

                        // neighbour
                        CurrentPin.SelectedNeighbourPin = iFiber.PinA;
                        iFiber.PinA.SelectedNeighbourPin = CurrentPin;

                        CurrentPin = iFiber.PinA; // update current pin to its neighbour/itself on SAME frame

                        FiberSearchFlag = true;

                        CurrentFiber = iFiber;
                        FiberFabricationIndexNote.Add(iFiber.FiberSortingIndex);

                        iFiber.LaidCount += 1; // Add connection info 

                        iFiber.PinA.ConnectedPins.Add(iFiber.PinB);
                        iFiber.PinB.ConnectedPins.Add(CurrentPin);

                        break;
                        
                    }

                    if (CurrentPin.FrameID == "B") // this fiber is from B-A
                    {
                        iFiber.Direction = "BA";

                        // neighbour
                        CurrentPin.SelectedNeighbourPin = iFiber.PinB;
                        iFiber.PinB.SelectedNeighbourPin = CurrentPin;

                        CurrentPin = iFiber.PinB;

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
