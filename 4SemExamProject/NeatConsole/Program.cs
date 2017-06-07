using DatabaseNormalizer;
using DatabaseNormalizer.DatabaseHandlers;
using NeatLib2;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            new Program().Run(inputData.NormalizedData, outputData.NormalizedData);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nDONE");
            Console.ReadLine();
        }

        private void Run(double[][] inputs, double[][] expectedOutputs)
        {
            //double[][] inputs = new double[][]
            //{
            //    new double[] { 1, 2 },
            //    new double[] { 2, 2 },
            //    new double[] { 3, 2 },
            //    new double[] { 4, 2 },
            //};

            //double[][] expectedOutputs = new double[][]
            //{
            //    new double[] { 2 },
            //    new double[] { 4 },
            //    new double[] { 6 },
            //    new double[] { 8 },
            //};

            Neat neat = new Neat();
            neat.OnGenerationEnd += (a) =>
            {
                Console.WriteLine(">  Gen " + a.Generation + "\tErr: " + a.Error);
            };

            Ann ann = neat.Train(1000, 100, 0.03, 2, 30, false, inputs, expectedOutputs, Crossover.TwoPointCrossover, ActivationFunction.Sigmoid);
            
            double finalError = neat.CalculateError(ann, inputs, expectedOutputs, true);
            Console.WriteLine("Final error: " + finalError);
            
            for (int j = 0; j < inputs.Length; j++)
            {
                double[] result = ann.Execute(inputs[j]);
                for (int i = 0; i < result.Length; i++)
                {
                    //Console.WriteLine($"Result {i}: {Math.Round(result[i], 2)}");
                }
            }
            Console.WriteLine($">>> Result: {Math.Round(ann.Execute(inputs[1])[0], 2)}");
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
            databaseLocation = databaseLocation.Replace(@"NeatConsole\bin\Debug\", @"DatabaseNormalizer\database\movie_metadata.csv");
            return DataManager.GetNormalizedDataFromDatabase(new DatabaseHandlerCSV(), databaseLocation, columns, condition, 0, 1);
        }
    }
}