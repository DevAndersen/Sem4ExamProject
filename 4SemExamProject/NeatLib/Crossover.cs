using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib
{
    public static class Crossover
    {
        public delegate Ann CrossoverOperation(Ann parentA, Ann parentB);

        public static Ann TwoPointCrossover(Ann parentA, Ann parentB)
        {
            bool parentRoll = Util.rand.Next(2) == 0;

            Ann primaryAnn = parentRoll ? parentA : parentB;
            Ann secondaryAnn = parentRoll ? parentB : parentA;
            Ann child = Util.CloneAnn(primaryAnn);
            child.hiddenNeurons.Clear();
            child.synapses.Clear();

            int neuronCount = primaryAnn.hiddenNeurons.Count;
            int synapseCount = primaryAnn.synapses.Count;

            int neuronSplit = Util.rand.Next(neuronCount);
            int synapseSplit = Util.rand.Next(synapseCount);

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

            for (int i = 0; i < synapseCount; i++)
            {
                if (i < synapseSplit)
                {
                    child.synapses.Add(primaryAnn.synapses[i]);
                }
                else
                {
                    if(i < secondaryAnn.synapses.Count)
                    {
                        child.synapses.Add(secondaryAnn.synapses[i]);
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