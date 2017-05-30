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
            Neat neat = new Neat(2, 2);
            neat.OnGenerationEnd += (a, g) =>
            {
                Console.WriteLine(">  Gen " + g + "\tErr: " + a.Error);
            };
            double[] inputs = new double[] { 1, 1 };
            double[] expectedOutputs = new double[] { 2, 4 };
            Ann ann = neat.Train(1000, 100, 0.05, 2, 1, false, inputs, expectedOutputs, Crossover.TwoPointCrossover);

            double[] result = ann.Execute(new double[] { 1, 1 });

            for (int i = 0; i < result.Length; i++)
            {
                Console.WriteLine($"Result {i}: {result[i]}");
            }
        }
    }
}