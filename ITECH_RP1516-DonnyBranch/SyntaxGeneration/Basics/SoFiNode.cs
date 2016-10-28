using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RP2k1516;
using Rhino.Geometry;

namespace RP1516
{
    public class SoFiNode
    {
        public double NodeNum;
        public Point3d Node;
        public double StressTen;
        public double StressCom; 
        public double Nx;
        public double Ny;
        public int Duplicate;

        public SoFiNode(double nodeNum, double x, double y, double z)
        {
            Node = new Point3d(x, y, z);
            NodeNum = nodeNum;
        }

        public static void InfoRead(List<SoFiNode> nodes, List<NodalStructureInfo> infos)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].NodeNum == infos[i].NodeNum)
                {
                    nodes[i].StressCom = infos[i].StressCom;
                    nodes[i].StressTen = infos[i].StressTen;
                    nodes[i].Nx = infos[i].Nx;
                    nodes[i].Ny = infos[i].Ny;
                }

                else
                {
                    for (int j = 0; j < 100; j++)
                    {
                        if (nodes[i].NodeNum == infos[i + j + 1].NodeNum)
                        {
                            nodes[i].StressCom = infos[i].StressCom;
                            nodes[i].StressTen = infos[i].StressTen;
                            nodes[i].Nx = infos[i].Nx;
                            nodes[i].Ny = infos[i].Ny;
                            break;
                        }
                    }
                }
            }
        }
    }
}
