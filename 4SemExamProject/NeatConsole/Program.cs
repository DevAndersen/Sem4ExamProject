using DatabaseNormalizer;
using DatabaseNormalizer.DatabaseHandlers;
using NeatLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DatabaseNormalizer.NormalizedDataAndDictionaries;

namespace NeatConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inputColumns = new string[]
            {
                //"movie_title",
                "genres",
                "director_name",
                "budget",
            };

            string[] expectedOutputColumns = new string[]
            {
                "gross",
            };

            NormalizedDataAndDictionaries inputData = new Program().GetDataFromDatabase(inputColumns);
            NormalizedDataAndDictionaries outputData = new Program().GetDataFromDatabase(expectedOutputColumns);

            new Program().Run(inputData.NormalizedData, outputData.NormalizedData, outputData.denormalizationVariablesList[0]);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nDONE");
            Console.ReadLine();
        }

        private void Run(double[][] inputs, double[][] expectedOutputs, DenormalizationVariables denormalizationVariables)
        {
            Neat neat = new Neat();
            neat.OnGenerationEnd += (a) =>
            {
                Console.WriteLine(">  Gen " + a.Generation + "\tErr: " + a.Error);
            };

            Ann ann = neat.Train(100, 100, 0.03, 2, 30, false, inputs, expectedOutputs, Crossover.TwoPointCrossover, ActivationFunction.Sigmoid);
            
            double finalError = neat.CalculateError(ann, inputs, expectedOutputs, true);
            Console.WriteLine("Final error: " + finalError);

            int index = 1;

            double normalizedResult = Math.Round(ann.Execute(inputs[index])[0], 2);
            double actualResult = DataManager.DenormalizeNumeric(normalizedResult, denormalizationVariables.NormalizedFloor, denormalizationVariables.NormalizedCeiling, denormalizationVariables.NumericNormalizationMargin, denormalizationVariables.SmallestTrainingValue, denormalizationVariables.LargestTrainingValue);
            double expectedResult = DataManager.DenormalizeNumeric(expectedOutputs[index][0], denormalizationVariables.NormalizedFloor, denormalizationVariables.NormalizedCeiling, denormalizationVariables.NumericNormalizationMargin, denormalizationVariables.SmallestTrainingValue, denormalizationVariables.LargestTrainingValue);

            Console.WriteLine($">>> Expected Normalized result: {expectedOutputs[index][0]}");
            Console.WriteLine($">>> Actual Normalized result: {normalizedResult}");
            Console.WriteLine($">>> Expected real result: {expectedResult}");
            Console.WriteLine($">>> Actual real result: {actualResult}");
            Console.WriteLine();
        }

        private void DoHardcodedTest()
        {
            NeatLib.Ann ann = new NeatLib.Ann(2, 1, NeatLib.ActivationFunction.Sigmoid);

            ann.hiddenNeurons.Add(new NeatLib.Neuron(0, 0) { Bias = -10 });
            ann.hiddenNeurons.Add(new NeatLib.Neuron(0, 1) { Bias = 30 });
            ann.outputNeurons[0].Bias = -30;

            ann.synapses.Add(new NeatLib.Synapse(-1, 0, 0, 0) { Weight = 20 });
            ann.synapses.Add(new NeatLib.Synapse(-1, 1, 0, 0) { Weight = 20 });

            ann.synapses.Add(new NeatLib.Synapse(-1, 0, 0, 1) { Weight = -20 });
            ann.synapses.Add(new NeatLib.Synapse(-1, 1, 0, 1) { Weight = -20 });

            ann.synapses.Add(new NeatLib.Synapse(0, 0, -2, 0) { Weight = 20 });
            ann.synapses.Add(new NeatLib.Synapse(0, 1, -2, 0) { Weight = 20 });

            Console.WriteLine("0 XOR 0 = " + Math.Round(ann.Execute(new double[] { 0, 0 })[0], 2));
            Console.WriteLine("0 XOR 1 = " + Math.Round(ann.Execute(new double[] { 0, 1 })[0], 2));
            Console.WriteLine("1 XOR 0 = " + Math.Round(ann.Execute(new double[] { 1, 0 })[0], 2));
            Console.WriteLine("1 XOR 1 = " + Math.Round(ann.Execute(new double[] { 1, 1 })[0], 2));

            Console.ReadLine();
        }

        private NormalizedDataAndDictionaries GetDataFromDatabase(string[] columns)
        {
            string condition = "WHERE actor_1_name = 'Robert De Niro'";
            string databaseLocation = AppDomain.CurrentDomain.BaseDirectory;
            databaseLocation = databaseLocation.Replace(@"NeatConsole\bin\Debug\", @"DatabaseNormalizer\database\4SemExamProject.mdf");
            return DataManager.GetNormalizedDataFromDatabase(new DatabaseHandlerSQL(), databaseLocation, columns, condition, 0, 1, 0.25);
        }
    }
}