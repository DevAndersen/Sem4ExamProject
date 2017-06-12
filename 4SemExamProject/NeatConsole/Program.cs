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
            Console.WriteLine("Demonstrating functionality: hardcoded XOR problem:");
            TestHardcodedXOR();

            Console.WriteLine("Demonstrating functionality: simple network exercise");
            TestWithSingleInput();

            Console.WriteLine("\nPress [ENTER] to execute actual NEAT.");
            Console.ReadLine();

            string[] inputColumns = new string[]
            {
                "genres",
                "director_name",
                "budget",
            };

            string[] expectedOutputColumns = new string[]
            {
                "gross",
            };

            NormalizedDataAndDictionaries inputData = GetDataFromDatabase(inputColumns);
            NormalizedDataAndDictionaries outputData = GetDataFromDatabase(expectedOutputColumns);

            RunNeat(inputData.NormalizedData, outputData.NormalizedData, outputData.denormalizationVariablesList[0]);
            Console.WriteLine("\nPress [ENTER] to terminate application.");
            Console.ReadLine();
        }

        private static void RunNeat(double[][] inputs, double[][] expectedOutputs, DenormalizationVariables denormalizationVariables)
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
            Console.WriteLine($">>> Error in USD: {actualResult - expectedResult}");
            Console.WriteLine();
        }

        private static void TestHardcodedXOR()
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

            double xor00 = Math.Round(ann.Execute(new double[] { 0, 0 })[0], 2);
            double xor01 = Math.Round(ann.Execute(new double[] { 0, 1 })[0], 2);
            double xor10 = Math.Round(ann.Execute(new double[] { 1, 0 })[0], 2);
            double xor11 = Math.Round(ann.Execute(new double[] { 1, 1 })[0], 2);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("XOR 0 + 0 = ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(xor00);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("XOR 0 + 1 = ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(xor01);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("XOR 1 + 0 = ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(xor10);

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("XOR 1 + 1 = ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(xor11);

            if(xor00 == 0 && xor01 == 1 && xor10 == 1 && xor11 == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Hardcoded neural network succesfully calculated XOR logic.");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Hardcoded neural network unsuccesfully calculated XOR logic.");
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine();
        }

        private static void TestWithSingleInput()
        {
            double[][] input =
            {
                new double[] { 1 },
                new double[] { 2 },
                new double[] { 3 },
                new double[] { 4 },
            };

            double[][] expectedOutput =
            {
                new double[] { 2 },
                new double[] { 4 },
                new double[] { 6 },
                new double[] { 8 },
            };

            Console.WriteLine("Attempting to teach how to double a number.");

            Neat neat = new Neat();
            neat.OnGenerationEnd += (a) =>
            {
                Console.WriteLine(">  Gen " + a.Generation + "\tErr: " + a.Error);
            };

            Ann ann = neat.Train(100, 100, 0.1, 1, 3, false, input, expectedOutput, Crossover.TwoPointCrossover, ActivationFunction.Identity);

            double result = ann.Execute(new double[] { 5 })[0];

            Console.WriteLine("> Input of 5, expected result of 10, real result: " + result);

        }

        private static NormalizedDataAndDictionaries GetDataFromDatabase(string[] columns)
        {
            string condition = "WHERE actor_1_name = 'Robert De Niro'";
            string databaseLocation = AppDomain.CurrentDomain.BaseDirectory;
            databaseLocation = databaseLocation.Replace(@"NeatConsole\bin\Debug\", @"DatabaseNormalizer\database\4SemExamProject.mdf");
            return DataManager.GetNormalizedDataFromDatabase(new DatabaseHandlerSQL(), databaseLocation, columns, condition, 0, 1, 0.25);
        }
    }
}