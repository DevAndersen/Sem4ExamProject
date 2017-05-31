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
                new double[] { 1, 1 }
            };

            double[][] expectedOutputs = new double[][]
            {
                new double[] { 2, 4 }
            };

            Neat neat = new Neat(2, 2);
            neat.OnGenerationEnd += (a) =>
            {
                Console.WriteLine(">  Gen " + a.Generation + "\tErr: " + a.Error);
            };

            Ann ann = neat.Train(1000, 100, 0.03, 3, 30, false, inputs, expectedOutputs, Crossover.TwoPointCrossover);
            
            double finalError = neat.CalculateError(ann, inputs, expectedOutputs);
            Console.WriteLine("Final error: " + finalError);

            double[] result = ann.Execute(inputs[0]);
            for (int i = 0; i < result.Length; i++)
            {
                Console.WriteLine($"Result {i}: {result[i]}");
            }
        }
    }
}