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

            SqlConnection connection = new SqlConnection()
            {
                ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\Martin\\4SemExamProject.mdf\";Integrated Security=True;Connection Timeout=600;"
            };

            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            cmd.CommandText = "SELECT movie_title, genres, actor_2_name, actor_3_name, director_name, budget, gross FROM moviedb WHERE actor_1_name = 'Robert Downey Jr.'";
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;

            connection.Open();

            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string[] strings =
                {
                    reader["movie_title"].ToString(),
                    reader["genres"].ToString().Split('|')[0],
                    //reader["actor_2_name"].ToString(),
                   // reader["actor_3_name"].ToString(),
                    reader["director_name"].ToString(),
                    reader["budget"].ToString(),
                    reader["gross"].ToString(),
                };
                string queryString = "";
                for (int i = 0; i < strings.Length; i++)
                {
                    queryString += strings[i] + (i == strings.Length - 1 ? "" : ", ");
                }
                Console.WriteLine(queryString);
            }
            reader.Close();
            connection.Close();

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
    }
}