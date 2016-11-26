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

            Point3d pt3 = new Point3d(pt1.X, pt1.Y, 0);
            Point3d pt4 = new Point3d(pt2.X, pt2.Y, 0);

            Line ll = new Line(pt3, pt4);
            Vector3d vv = new Vector3d(0.0, 0.0, 10.0);
            flag = Intersection.SurfaceSurface(Surface.CreateExtrusion(ll.ToNurbsCurve(), vv), srf, 0.001, out intersectCrv, out intersectPt);

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
                   
                    Curve fiberCrv = GeodesicLine(srf, pinsA[i].Position, pinsB[j].Position);
                    fiberCrvs.Add(fiberCrv);

                    Fiber ifiber = new Fiber(fiberCrv, -1, "AB");
                    ifiber.StartPinID = pinsA[i].PinID;
                    ifiber.EndPinID = pinsB[j].PinID; // always starts from A to B
                    ifiber.PinA = pinsA[i];
                    ifiber.PinB = pinsB[j];

                    pinsA[i].VisibleFibers.Add(ifiber);
                    pinsB[j].VisibleFibers.Add(ifiber);

                    fibers.Add(ifiber);
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

        static public List<Point3d> PublicatePointsOnCurve(Curve crv, int count) // by count
        {
            List<Point3d> Pts = new List<Point3d>();
            for(int i = 0; i < count; i++)
            {
                double pointat = (double)i / (double)count;
                Pts.Add(crv.PointAt(pointat));
            }
            
            return Pts;
        }

    }

}

