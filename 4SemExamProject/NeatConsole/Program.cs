using DatabaseNormalizer;
using NeatLib;
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
            Ann ann = new Ann(2, 1, ActivationFunction.Sigmoid);

            ann.hiddenNeurons.Add(new Neuron(0, 0) { Bias = -10 });
            ann.hiddenNeurons.Add(new Neuron(0, 1) { Bias = 30 });
            ann.outputNeurons[0].Bias = -30;

            ann.synapses.Add(new Synapse(-1, 0, 0, 0) { Weight = 20 });
            ann.synapses.Add(new Synapse(-1, 1, 0, 0) { Weight = 20 });

            ann.synapses.Add(new Synapse(-1, 0, 0, 1) { Weight = -20 });
            ann.synapses.Add(new Synapse(-1, 1, 0, 1) { Weight = -20 });

            ann.synapses.Add(new Synapse(0, 0, -2, 0) { Weight = 20 });
            ann.synapses.Add(new Synapse(0, 1, -2, 0) { Weight = 20 });

            Console.WriteLine("0 XOR 0 = " + Math.Round(ann.Execute(new double[] { 0, 0 })[0], 2));
            Console.WriteLine("0 XOR 1 = " + Math.Round(ann.Execute(new double[] { 0, 1 })[0], 2));
            Console.WriteLine("1 XOR 0 = " + Math.Round(ann.Execute(new double[] { 1, 0 })[0], 2));
            Console.WriteLine("1 XOR 1 = " + Math.Round(ann.Execute(new double[] { 1, 1 })[0], 2));

            Console.ReadLine();

            new Program().Run();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nDONE");
            Console.ReadLine();
        }

        private void Run()
        {
            double[][] inputs = new double[][]
            {
                new double[] { 0, 0 },
                new double[] { 0, 1 },
                new double[] { 1, 0 },
                new double[] { 1, 1 },
            };

            double[][] expectedOutputs = new double[][]
            {
                new double[] { 0 },
                new double[] { 1 },
                new double[] { 1 },
                new double[] { 0 },
            };

            Neat neat = new Neat();
            neat.OnGenerationEnd += (a) =>
            {
                Console.WriteLine(">  Gen " + a.Generation + "\tErr: " + a.Error);
            };

            Ann ann = neat.Train(100, 100, 0.03, 3, 30, false, inputs, expectedOutputs, Crossover.BestParentClone, ActivationFunction.Sigmoid);
            
            double finalError = neat.CalculateError(ann, inputs, expectedOutputs, true);
            Console.WriteLine("Final error: " + finalError);
            
            for (int j = 0; j < inputs.Length; j++)
            {
                double[] result = ann.Execute(inputs[j]);
                for (int i = 0; i < result.Length; i++)
                {
                    Console.WriteLine($"Result {i}: {Math.Round(result[i], 2)}");
                }
            }
            Console.WriteLine();
        }

        private List<Dictionary<string, double>> GetDataFromDatabase()
        {
            string[] columns = new string[]
            {
                //"movie_title",
                "genres",
                "director_name",
                "budget",
                "gross"
            };

            string condition = "WHERE actor_1_name = 'Robert Downey Jr.'";
            string databaseLocation = AppDomain.CurrentDomain.BaseDirectory;
            databaseLocation = databaseLocation.Replace(@"bin\Debug\", @"database\4SemExamProject.mdf");
            DatabaseHandler dbh = new DatabaseHandler();
            return dbh.GetNormalizedData(databaseLocation, columns, condition, 0, 1);
        }
    }
}