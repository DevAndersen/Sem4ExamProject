using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib2
{
    public static class Crossover
    {
        public delegate Ann CrossoverOperation(Ann parentA, Ann parentB);

        public static Ann TwoPointCrossover(Ann parentA, Ann parentB)
        {
            bool parentRoll = Util.rand.Next(2) == 0;

            Ann primaryAnn = Util.CloneAnn(parentRoll ? parentA : parentB);
            Ann secondaryAnn = Util.CloneAnn(parentRoll ? parentB : parentA);
            Ann child = Util.CloneAnn(primaryAnn);
            child.hiddenNeurons.Clear();

            int neuronCount = primaryAnn.hiddenNeurons.Count;

            int neuronSplit = Util.rand.Next(neuronCount);

            for (int i = 0; i < neuronCount; i++)
            {
                if(i < neuronSplit)
                {
                    child.hiddenNeurons.Add(primaryAnn.hiddenNeurons[i]);
                }
                else
                {
                    if (i < secondaryAnn.hiddenNeurons.Count)
                    {
                        child.hiddenNeurons.Add(secondaryAnn.hiddenNeurons[i]);
                    }
                }
            }

            return child;
        }

        public static Ann BestParentClone(Ann parentA, Ann parentB)
        {
            return Util.CloneAnn(parentA);
        }
    }
}