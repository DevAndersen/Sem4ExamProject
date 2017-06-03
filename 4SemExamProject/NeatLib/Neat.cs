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
        
        public delegate void AnnEventHandler(Ann ann);
        public event AnnEventHandler OnGenerationEnd;

        private bool waitFlag = false;

        public Ann Train(int maxGenerations, int generationSize, double? errorThreshold, int mutationRate, int mutationRolls, bool waitForFlag, double[][] inputs, double[][] expectedOutputs, Crossover.CrossoverOperation crossoverOperation, ActivationFunction.ActivationMethod activationMethod)
        {
            Ann[] generation = new Ann[generationSize];
            int inputCount = inputs[0].Length;
            int outputCount = expectedOutputs[0].Length;

            for (int generationId = 0; generationId < maxGenerations; generationId++)
            {
                if(generationId == 0)
                {
                    for (int startingIndividual = 0; startingIndividual < generationSize; startingIndividual++)
                    {
                        Ann ann = new Ann(inputCount, outputCount, activationMethod);
                        double error = CalculateError(ann, inputs, expectedOutputs);
                        ann.Error = error;
                        generation[startingIndividual] = ann;
                    }
                }
                else
                {
                    Ann parentA = generation[0];
                    Ann parentB = generation[1];

                    for (int newIndividual = 0; newIndividual < generationSize; newIndividual++)
                    {
                        generation = generation.OrderByDescending(x => x.Error).ToArray();

                        Ann child = crossoverOperation.Invoke(parentA, parentB);
                        Mutations.RollToCauseRandomMutation(child, mutationRate, mutationRolls);
                        double error = CalculateError(child, inputs, expectedOutputs);
                        child.Error = error;

                        if(child.Error < generation[0].Error)
                            generation[0] = child;
                    }
                }

                generation.ToList().ForEach(x =>
                {
                    x.Error = CalculateError(x, inputs, expectedOutputs);
                    x.Generation = generationId;
                });

                generation = generation.OrderBy(x => x.Error).ToArray();

                if(waitForFlag)
                {
                    waitFlag = true;
                    OnGenerationEnd?.Invoke(generation[0]);
                    while (waitFlag)
                    {

                    }
                }
                else
                {
                    OnGenerationEnd?.Invoke(generation[0]);
                }

                if (errorThreshold != null && generation[0].Error < errorThreshold)
                    return generation[0];
            }
            return generation[0];
        }

        public double CalculateError(Ann ann, double[][] inputs, double[][] expectedOutputs, bool debugFlag = false)
        {
            double error = 0;

            for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
            {
                double[] currentInputs = inputs[inputIndex];
                double[] currentExpectedOutputs = expectedOutputs[inputIndex];

                double[] outputs = ann.Execute(currentInputs);

                for (int i = 0; i < currentExpectedOutputs.Length; i++)
                {
                    error += Math.Sqrt(Math.Pow(currentExpectedOutputs[i] - outputs[i], 2));
                }

                for (int i = 0; i < ann.hiddenNeurons.Count + ann.synapses.Count; i++)
                {
                    error += ErrorPerPart;
                }
            }

            return error;
        }

        public void LowerWaitFlag()
        {
            waitFlag = false;
        }
    }
}