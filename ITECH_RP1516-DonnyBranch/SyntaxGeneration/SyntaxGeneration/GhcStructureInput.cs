using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper;
using Rhino.Geometry;
using RP2k1516;
using System.Linq;

namespace RP1516
{
    public class GhcStructureInput : GH_Component
    {

        public GhcStructureInput()
          : base("StructureInput", "StructureInput",
              "StructureInput",
              "RP1516", "SyntaxGeneration")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Node", "Node", "Node", GH_ParamAccess.list);
            pManager.AddNumberParameter("X", "X", "X", GH_ParamAccess.list);
            pManager.AddNumberParameter("Y", "Y", "Y", GH_ParamAccess.list);
            pManager.AddNumberParameter("Z", "Z", "Z", GH_ParamAccess.list);

            pManager.AddNumberParameter("Node Number", "Node Number", "Node Number", GH_ParamAccess.list);
            pManager.AddNumberParameter("Nodal Tension Stress", "Nodal Tension Stress", "Nodal Tension Stress", GH_ParamAccess.list);
            pManager.AddNumberParameter("Nodal Compression Stress", "Nodal Compression Stress", "Nodal Compression Stress", GH_ParamAccess.list);
            pManager.AddNumberParameter("Nodal Force Nx", "Nodal Force Nx", "Nodal Force Nx", GH_ParamAccess.list);
            pManager.AddNumberParameter("Nodal Force Ny", "Nodal Force Ny", "Nodal Force Ny", GH_ParamAccess.list);

            pManager.AddGenericParameter("Custom Fibers", "Custom Fibers", "Custom Fibers", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Surface", "Surface", "Surface", GH_ParamAccess.item);

            //pManager.AddMeshParameter("Mesh", "Mesh", "Mesh", GH_ParamAccess.item);
            //pManager.AddGenericParameter("Fiber Big List", "Fiber Big List", "Fiber Big List", GH_ParamAccess.list);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SoFiNodes", "SoFiNodes", "SoFiNodes", GH_ParamAccess.list);
            pManager.AddPointParameter("Node Positions", "Node Positions", "Node Positions", GH_ParamAccess.list);
            pManager.AddNumberParameter("Node Number", "Node Number", "Node Number", GH_ParamAccess.list);
            pManager.AddNumberParameter("Info Number", "Info Number", "Info Number", GH_ParamAccess.list);
            pManager.AddCurveParameter("Criticle Fibers", "Criticle Fibers", "Criticle Fibers", GH_ParamAccess.list);
            pManager.AddNumberParameter("Value", "Value", "Value", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Fibers to Present", "Fibers to Present", "Fibers to Present", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Fiber Info", "Fiber Info", "Fiber Info", GH_ParamAccess.item); 
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> node = new List<double>();
            List<double> nodeX = new List<double>();
            List<double> nodeY = new List<double>();
            List<double> nodeZ = new List<double>();

            List<double> nodeNum = new List<double>();
            List<double> stressesTen = new List<double>();
            List<double> stressesCom = new List<double>();
            List<double> nx = new List<double>();
            List<double> ny = new List<double>();

            List<Fiber> Fibers = new List<Fiber>();

            Surface Srf4Nodes = null;

            DA.GetDataList("X", nodeX);
            DA.GetDataList("Y", nodeY);
            DA.GetDataList("Z", nodeZ);
            DA.GetDataList("Node", node); // small

            DA.GetDataList("Node Number", nodeNum); // big (duplicate)
            DA.GetDataList("Nodal Tension Stress", stressesTen);
            DA.GetDataList("Nodal Compression Stress", stressesCom);
            DA.GetDataList("Nodal Force Nx", nx);
            DA.GetDataList("Nodal Force Ny", ny);

            DA.GetDataList("Custom Fibers", Fibers);
            DA.GetData("Surface", ref Srf4Nodes);

            // SoFiNode: structure info assambly
            List<SoFiNode> SoFiNodesOnSrf = new List<SoFiNode>();

            for (int i = 0; i < nodeX.Count; i++)
            {
                SoFiNode inode = new SoFiNode(node[i], nodeX[i], nodeY[i], nodeZ[i]);
                double u;
                double v;
                Srf4Nodes.ClosestPoint(inode.Node, out u, out v);
                Point3d closestPt = Srf4Nodes.PointAt(u, v);

                if ( closestPt.DistanceTo(inode.Node) < 0.01) // on surface
                {
                    SoFiNodesOnSrf.Add(inode);
                }
            }
            // so far the sofiNodes have no structure info  

            // ====================== structure info big list ===================================
            // node number, compression stress, tension stress, force in  local x, force in local y 

            List<NodalStructureInfo> NodalStructureInfo = new List<NodalStructureInfo>();
            for (int i = 0; i < nodeNum.Count; i++) 
            {
                NodalStructureInfo inodal = new NodalStructureInfo(nodeNum[i], stressesTen[i], stressesCom[i], nx[i], ny[i]);
                NodalStructureInfo.Add(inodal);
            }
            // ==================================================================================

            // abandom the nodes not on the srf
            // list.RemoveAll(item => !list2.Contains(item))
            List<double> nodeNumOnSrf = SoFiNodesOnSrf.Select(o => o.NodeNum).ToList();
            NodalStructureInfo.RemoveAll(o => !nodeNumOnSrf.Contains(o.NodeNum)); // remove structure info not on srf
            List<NodalStructureInfo> CondensedNodalStructureInfo = new List<RP1516.NodalStructureInfo>();

            for (int i = 0; i < SoFiNodesOnSrf.Count; i++)
            {
                double iNodeNumber = SoFiNodesOnSrf[i].NodeNum;
                List<NodalStructureInfo> iNodalinfo = NodalStructureInfo.Where(o => o.NodeNum == iNodeNumber).ToList();
                double iNodeCount = iNodalinfo.Count();

                // Com
                double iComStress = iNodalinfo
                    .Select(o => o.StressCom).ToList().Sum() 
                    / iNodeCount;

                // Ten
                double iTenStress = iNodalinfo
                    .Select(o => o.StressTen).ToList().Sum()
                    / iNodeCount;

                // NX
                double iNX = iNodalinfo
                    .Select(o => o.Nx).ToList().Sum()
                    / iNodeCount;

                // NY
                double iNY = iNodalinfo
                    .Select(o => o.Ny).ToList().Sum()
                    / iNodeCount;

                NodalStructureInfo.Except(iNodalinfo);

                SoFiNodesOnSrf[i].StressCom = iComStress;
                SoFiNodesOnSrf[i].StressTen = iTenStress;
                SoFiNodesOnSrf[i].Nx = iNX;
                SoFiNodesOnSrf[i].Ny = iNY;

            }
            //List<NodalStructureInfo> CondensedNodalStructureInfo = Utils.CheckDuplicate(NodalStructureInfo);

            // SoFiNode.InfoRead(SoFiNodesOnSrf, CondensedNodalStructureInfo);
             
            // calculate structure significance
            Fibers.ForEach(o => o.StrutureValue(SoFiNodesOnSrf));

            List<Fiber> CriticleFibers = Fibers
                .OrderByDescending(o => o.StructureFactor)
                //.Take((int)(Fibers.Count * 0.2))
                .ToList();

            // Output
            DA.SetDataList("SoFiNodes", SoFiNodesOnSrf);
            DA.SetDataList("Node Positions", SoFiNodesOnSrf.Select(o => o.Node).ToList());
            DA.SetDataList("Node Number", SoFiNodesOnSrf.Select(o => o.NodeNum).ToList());
            DA.SetDataList("Info Number", CondensedNodalStructureInfo.Select(o => o.NodeNum).ToList());
            DA.SetDataList("Criticle Fibers", CriticleFibers.Select(o => o.FiberCrv).ToList());
            DA.SetDataList("Value", SoFiNodesOnSrf.Select(o => o.StressCom).ToList());

        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return null;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{aaf690bc-6815-4447-a896-6ec74d4ee8a6}"); }
        }
    }
}