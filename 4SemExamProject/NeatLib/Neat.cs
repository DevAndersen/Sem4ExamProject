using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib
{
    public class Neat
    {
        private const double ErrorPerPart = 0.0;
        
        public delegate void AnnEventHandler(Ann ann, int generation);
        public event AnnEventHandler OnGenerationEnd;

        private bool waitFlag = false;

        private int inputCount;
        private int outputCount;

        public Neat(int InputCount, int OutputCount)
        {
            inputCount = InputCount;
            outputCount = OutputCount;
        }

        public Ann Train(int maxGenerations, int generationSize, double? errorThreshold, int mutationRate, int mutationRolls, bool waitForFlag, double[] inputs, double[] expectedOutputs, Crossover.CrossoverOperation crossoverOperation)
        {
            Ann[] generation = new Ann[generationSize];

            for (int generationId = 0; generationId < maxGenerations; generationId++)
            {
                //Console.WriteLine("Generation: " + generationId);
                if(generationId == 0)
                {
                    for (int startingIndividual = 0; startingIndividual < generationSize; startingIndividual++)
                    {
                        generation[startingIndividual] = new Ann(inputCount, outputCount);
                    }
                }
                else
                {
                    Ann parentA = generation[0];
                    Ann parentB = generation[1];

                    for (int newIndividual = 0; newIndividual < generationSize; newIndividual++)
                    {
                        //generation[newIndividual] = Ann.CreateOffspring(parentA, parentB, 2, 1);
                        //generation[newIndividual] = Util.CloneAnn(parentA);
                        //Ann.RollToCauseRandomMutation(generation[newIndividual], 2, 1);
                        Ann child = crossoverOperation.Invoke(parentA, parentB);
                        Mutations.RollToCauseRandomMutation(child, mutationRate, mutationRolls);
                        generation[newIndividual] = child;
                    }
                }

                for (int individual = 0; individual < generationSize; individual++)
                {
                    Ann ann = generation[individual];
                    double[] outputs = ann.Execute(inputs);
                    double error = CalculateError(ann, outputs, expectedOutputs);
                    ann.Error = error;
                    ann.Generation = generationId + 1;
                }
                generation = generation.OrderBy(x => x.Error).ToArray();

                if(waitForFlag)
                {
                    waitFlag = true;
                    OnGenerationEnd?.Invoke(generation[0], generationId);
                    while (waitFlag)
                    {

                    }
                }
                else
                {
                    OnGenerationEnd?.Invoke(generation[0], generationId);
                }

                if (errorThreshold != null && generation[0].Error < errorThreshold)
                    return generation[0];
            }
            return generation[0];
        }

        public double CalculateError(Ann ann, double[] actualOutputs, double[] expectedOutputs)
        {
            if (expectedOutputs.Length != actualOutputs.Length)
                throw new ArgumentOutOfRangeException("The number of expected outputs must match the number of actual outputs.");

            double error = 0;

            for (int i = 0; i < expectedOutputs.Length; i++)
            {
                error += Math.Sqrt(Math.Pow(expectedOutputs[i] - actualOutputs[i], 2));
            }

            for (int i = 0; i < ann.hiddenNeurons.Count + ann.synapses.Count; i++)
            {
                error += ErrorPerPart;
            }

            return error;
        }

        public void LowerWaitFlag()
        {
            waitFlag = false;
        }
    }
}