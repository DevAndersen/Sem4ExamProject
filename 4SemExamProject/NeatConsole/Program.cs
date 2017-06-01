using NeatLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nDONE");
            Console.ReadLine();
        }

        private void Run()
        {
            double[][] inputs = new double[][]
            {
                new double[] { 10 }
            };

            double[][] expectedOutputs = new double[][]
            {
                new double[] { 2, 4 }
            };

            inputs = new double[][]
            {
                new double[] { 10, 10 }
            };

            expectedOutputs = new double[][]
            {
                new double[] { 20, 40, 60 }
            };

            Neat neat = new Neat();
            neat.OnGenerationEnd += (a) =>
            {
                Console.WriteLine(">  Gen " + a.Generation + "\tErr: " + a.Error);
            };

            Ann ann = neat.Train(1000, 100, 0.03, 3, 30, false, inputs, expectedOutputs, Crossover.TwoPointCrossover, ActivationFunction.Identity);
            
            double finalError = neat.CalculateError(ann, inputs, expectedOutputs);
            Console.WriteLine("Final error: " + finalError);

            double[] result = ann.Execute(new double[] { 5, 5 });
            for (int i = 0; i < result.Length; i++)
            {
                Console.WriteLine($"Result {i}: {result[i]}");
            }
        }
    }
}