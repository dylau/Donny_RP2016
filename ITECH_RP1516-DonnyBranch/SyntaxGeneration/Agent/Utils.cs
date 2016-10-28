using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;



namespace RP1516
{
    static class Utils // i.e. Utilities
    {
        /*
        static Random randomGenerator = new Random();
        
        static public Point3d GetRandomPoint(double minX, double maxX, double minY, double maxY, double minZ, double maxZ)
        {
            double x = minX + (maxX - minX) * randomGenerator.NextDouble();
            double y = minY + (maxY - minY) * randomGenerator.NextDouble();
            double z = minZ + (maxZ - minZ) * randomGenerator.NextDouble();

            return new Point3d(x, y, z);
        }

        static public Vector3d GetRandomUnitVectorXY()
        {
            double alpha = randomGenerator.NextDouble() * 2.0 * Math.PI;
            return new Vector3d(Math.Cos(alpha), Math.Sin(alpha), 0.0);
        }

        static public Vector3d GetRandomUnitVector()
        {
            double phi = randomGenerator.NextDouble() * 2.0 * Math.PI;
            double theta = Math.Acos(randomGenerator.NextDouble() * 2.0 - 1.0);

            double x = Math.Sin(theta) * Math.Cos(phi);
            double y = Math.Sin(theta) * Math.Sin(phi);
            double z = Math.Cos(theta);

            return new Vector3d(x, y, z);
        }
        */

        static public Curve GeodesicLine(Surface srf, Point3d pt1, Point3d pt2)
        {
            Curve geodesicLine = null;

            double pt1U;
            double pt1V;
            srf.ClosestPoint(pt1, out pt1U, out pt1V);
            Point2d pt12d = new Point2d(pt1U, pt1V);

            double pt2U;
            double pt2V;
            srf.ClosestPoint(pt2, out pt2U, out pt2V);
            Point2d pt22d = new Point2d(pt2U, pt2V);

            geodesicLine = srf.ShortPath(pt12d, pt22d, 0.01);

            return geodesicLine;
        }

        static public Curve[] IntersectionCrv(Surface srf, Point3d pt1, Point3d pt2)
        {
            Curve[] intersectCrv;
            Point3d[] intersectPt;

            bool flag;

            /*
            Point3d pt3 = new Point3d(pt1.X, pt1.Y, 0); // define pt3 as a point above pt1 to define a plane
            Point3d pt4 = new Point3d(pt2.X, pt2.Y, 0);
            flag = Intersection.BrepSurface(Brep.CreateFromCornerPoints(pt1, pt2, pt3, pt4, 0.001), // intersaction plane
                srf, 0.001, out intersectCrv, out intersectPt);
            */

            // !!!

            Point3d pt3 = new Point3d(pt1.X, pt1.Y, 0);
            Point3d pt4 = new Point3d(pt2.X, pt2.Y, 0);

            Line ll = new Line(pt3, pt4);
            Vector3d vv = new Vector3d(0.0, 0.0, 10.0);
            flag = Intersection.SurfaceSurface(Surface.CreateExtrusion(ll.ToNurbsCurve(), vv), srf, 0.001, out intersectCrv, out intersectPt);

            //Brep InputSrf2Brep = new Brep();
            //InputSrf2Brep = srf.ToBrep();

            // !!!
            //flag = Intersection.BrepBrep(Brep.CreateFromCornerPoints(pt1, pt2, pt3, pt4, 0.001), // intersaction plane
            //    InputSrf2Brep, 0.001, out intersectCrv, out intersectPt);
            // !!!


            if (flag)
                return intersectCrv;
            else
                return null;
        }

        static public List<Fiber> GenerateFibers(List<Pin> pinsA, List<Pin> pinsB, Surface srf, string fiberGenerationType)
        {
            List<Fiber> fibers = new List<Fiber>();
            List<Curve> fiberCrvs = new List<Curve>();

            for (int i = 0; i < pinsA.Count; i++) //A
            {
                for (int j = 0; j < pinsB.Count; j++) //B
                {
                    if (fiberGenerationType == "Intersaction")
                    {
                        // intersection
                        Curve[] IntersactionCrv = IntersectionCrv(srf, pinsA[i].Position, pinsB[j].Position); // out IntersectionCrvResult
                        Curve fiberCrv = null;
                        if (IntersactionCrv == null || IntersactionCrv.Length == 0)
                            break;
                        else
                            fiberCrv = IntersactionCrv.Where(o => o != null).ToList().First();

                        if ((fiberCrv.PointAtStart.DistanceTo(pinsA[i].Position) < 0.01 && fiberCrv.PointAtEnd.DistanceTo(pinsB[j].Position) < 0.01)) 
                        {
                            fiberCrvs.Add(fiberCrv);

                            Fiber ifiber = new Fiber(fiberCrv, -1, "AB", "MAT");
                            ifiber.StartPinID = pinsA[i].PinID;
                            ifiber.EndPinID = pinsB[j].PinID; // always starts from A to B
                            ifiber.PinA = pinsA[i];
                            ifiber.PinB = pinsB[j];
                            //ifiber.StartPin = pinsA[i];
                            //ifiber.EndPin = pinsB[i];

                            pinsA[i].VisibleFibers.Add(ifiber);
                            pinsB[j].VisibleFibers.Add(ifiber);

                            fibers.Add(ifiber);
                        }
                            // (fiberCrv.PointAtStart.DistanceTo(pinsB[j].Position) < 0.01 && fiberCrv.PointAtEnd.DistanceTo(pinsA[i].Position) < 0.01))
                        //{
                            
                       // }
                        //else
                          //  break;
                    }

                    if (fiberGenerationType == "Geodesic") // geodesic
                    {
                        Curve fiberCrv = GeodesicLine(srf, pinsA[i].Position, pinsB[j].Position);
                        fiberCrvs.Add(fiberCrv);

                        Fiber ifiber = new Fiber(fiberCrv, -1, "AB", "MAT");
                        ifiber.StartPinID = pinsA[i].PinID;
                        ifiber.EndPinID = pinsB[j].PinID; // always starts from A to B
                        ifiber.PinA = pinsA[i];
                        ifiber.PinB = pinsB[j];

                        pinsA[i].VisibleFibers.Add(ifiber);
                        pinsB[j].VisibleFibers.Add(ifiber);

                        fibers.Add(ifiber);
                    }

                    
                        
                }
            }

            return fibers;

        }
        
        static public List<NodalStructureInfo> CheckDuplicate(List<NodalStructureInfo> nodalInfo) 
        {
            List<NodalStructureInfo> CondensedNodalStructureInfo = new List<NodalStructureInfo>();

            for (int i = 0; i < nodalInfo.Count; i++)
            {
                if (( i + 1 < nodalInfo.Count) && (nodalInfo[i].NodeNum == nodalInfo[i + 1].NodeNum))
                {
                    NodalStructureInfo mergedNodalIndo = NodalStructureInfo.MergeDuplicate(nodalInfo[i], nodalInfo[i + 1]);
                    CondensedNodalStructureInfo.Add(mergedNodalIndo);
                    i += 1;
                }

                else
                {
                    CondensedNodalStructureInfo.Add(nodalInfo[i]);
                }
            }
            return CondensedNodalStructureInfo;
        }

        static public double DistancePointToCurve(Point3d p, Curve c)
        {
            double dis = 0.0;
            double t;
            Point3d pOnCurve = new Point3d();

            c.ClosestPoint(p, out t);
            pOnCurve = c.PointAt(t);
            dis = pOnCurve.DistanceTo(p);
            return dis;
        }
        /*
        static public List<TurningPoint> FindTurningPt(Curve crv)
        {
            List<TurningPoint> turningPts = new List<TurningPoint>();
            double t = 0.0;
            List<Vector3d> tangents = new List<Vector3d>();
            int divide = 100;
            for (int i = 0; i < divide; i++)
            {
                t = (double)i / (double)(divide - 1);
                Vector3d tg = crv.TangentAt(t);
                
                tangents.Add(tg);
                if (i > 0)
                {
                    if (tangents[i-1].Z >= 0 && tangents[i].Z <= 0)
                    {
                        double tt = (t + (double)(i - 1) / (double)(divide - 1)) / 2; // use middle pt to approximate the turning pt t
                        TurningPoint tnPt = new TurningPoint(crv.PointAt(tt), "min", tt);
                        turningPts.Add(tnPt);
                    }

                    if (tangents[i - 1].Z <= 0 && tangents[i].Z >= 0)
                    {
                        double tt = (t + (double)(i - 1) / (double)(divide - 1)) / 2; // use middle pt to approximate the turning pt t
                        TurningPoint tnPt = new TurningPoint(crv.PointAt(tt), "max", tt);
                        turningPts.Add(tnPt);
                    }
                }
            }

            return turningPts;

        } // cannot find max or min
        */

        static public List<Point3d> PublicatePointsOnCurve(Curve crv, int count) // by count
        {
            
            List<Point3d> Pts = new List<Point3d>();
            List<double> tt = new List<double>();

            tt = crv.DivideByLength(0.1, true).ToList();
            foreach(double t in tt)
            {
                Pts.Add(crv.PointAt(t));

            }

            return Pts;
        }

        static public List<Point3d> PublicatePointsOnCurve(Curve crv, double len) // by length
        {
            List<Point3d> Pts = new List<Point3d>();
            List<double> tt = new List<double>();

            tt = crv.DivideByLength(len, true).ToList();

            tt = crv.DivideByLength(0.1, true).ToList();
            foreach (double t in tt)
            {
                Pts.Add(crv.PointAt(t));

            }

            return Pts;

        }

        static public string DigitControl(int targetDigit, int originInteger)
        {
            string theString = null;

            return theString;

        }
       

    }

}

