using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RP2k1516;
using Rhino.Geometry;



namespace RP1516
{
    public class NodalStructureInfo
    {
        public double NodeNum;
        public double StressTen;
        public double StressCom;
        public double Nx;
        public double Ny;

        public NodalStructureInfo(double nodeNum, double stressTen, double stressCom, double nx, double ny)
        {
            NodeNum = nodeNum;
            StressTen = stressTen;
            StressCom = stressCom;
            Nx = nx;
            Ny = ny;
         }

        public static NodalStructureInfo  MergeDuplicate(NodalStructureInfo infoA, NodalStructureInfo infoB) 
        {
            NodalStructureInfo mergedInfo = new NodalStructureInfo(infoA.NodeNum,
                                                                   (infoA.StressTen + infoB.StressTen) / 2,
                                                                   (infoA.StressCom + infoB.StressCom) / 2,
                                                                   (infoA.Nx + infoB.Nx) / 2,
                                                                   (infoA.Ny + infoB.Ny) / 2
                                                                   );
            return mergedInfo;

        }

                #region
                /*
                for (int i = 0; i < nodeNum.Count; i++)
                {
                    if (nodeNum[i] == nodeNum[i+1])
                    {
                        double evenStress = (stresses[i] + stresses[i + 1]) / 2;
                        nodeNumChecked.Add(nodeNum[i]);
                        stressesChecked.Add(evenStress);

                        SoFiNode iNode = new SoFiNode(nodeNum[i], nodeX[i], nodeY[i], nodeZ[i], evenStress);
                        SoFiNodes.Add(iNode);
                    }

                    else
                    {
                        if ((i-1 >= 0) && (nodeNum[i] != nodeNum[i - 1]))
                        {
                            nodeNumChecked.Add(nodeNum[i]);
                            SoFiNode iNode = new SoFiNode(nodeNum[i], nodeX[i], nodeY[i], nodeZ[i], stresses[i]);
                            SoFiNodes.Add(iNode);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                */
                #endregion
    }
}

    




