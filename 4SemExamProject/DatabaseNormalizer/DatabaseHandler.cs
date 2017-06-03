using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseNormalizer
{
    public class DatabaseHandler
    {
        public NormalizedDataAndDictionaries GetNormalizedData(string databaseLocation, string[] columns, string queryConditions, double normalizationMinimum, double normalizationMaximum)
        {
            List<string[]> dataFromDatabase = GetDataFromDatabase(databaseLocation, columns, queryConditions);

            return NormalizeData(dataFromDatabase, columns, normalizationMinimum, normalizationMaximum);
        }

        private List<string[]> GetDataFromDatabase(string databaseLocation, string[] columns, string queryConditions)
        {
            SqlConnection connection = new SqlConnection()
            {
                ConnectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"" + databaseLocation + "\";Integrated Security=True;Connection Timeout=600;"
            };

            SqlCommand cmd = new SqlCommand();
            SqlDataReader reader;

            string columnList = "";

            for (int i = 0; i < columns.Length; i++)
            {
                columnList += columns[i] + (i == columns.Length - 1 ? "" : ", ");
            }

            cmd.CommandText = "SELECT " + columnList + " FROM moviedb " + queryConditions;
            cmd.CommandType = CommandType.Text;
            cmd.Connection = connection;

            connection.Open();

            reader = cmd.ExecuteReader();

            List<string[]> resultStrings = new List<string[]>();

            while (reader.Read())
            {
                string[] strings = new string[columns.Length];

                for (int i = 0; i < columns.Length; i++)
                {
                    object data = reader[columns[i]];
                    string dataString = "";

                    if (data.ToString().Contains('|'))
                    {
                        dataString = reader[columns[i]].ToString().Split('|')[0];
                    }
                    else
                    {
                        dataString = reader[columns[i]].ToString();
                    }

                    strings[i] = dataString;
                }
                string[] resultString = new string[columns.Length];
                for (int i = 0; i < strings.Length; i++)
                {
                    resultString[i] = strings[i];
                }
                resultStrings.Add(resultString);
            }
            reader.Close();
            connection.Close();

            return resultStrings;
        }

        private NormalizedDataAndDictionaries NormalizeData(List<string[]> dataFromDatabase, string[] columns, double normalizationMinimum, double normalizationMaximum)
        {
            Dictionary<string, double>[] dataDictionaries = new Dictionary<string, double>[columns.Length];

            for (int i = 0; i < columns.Length; i++)
            {
                int counter = 0;
                Dictionary<string, double> dictionary = new Dictionary<string, double>();
                foreach (string[] resultString in dataFromDatabase)
                {
                    string result = resultString[i];
                    if (!dictionary.ContainsKey(result))
                    {
                        dictionary.Add(result, counter);
                        counter++;
                    }
                }
                dataDictionaries[i] = dictionary;
            }

            foreach (Dictionary<string, double> dictionary in dataDictionaries)
            {
                for (int i = 0; i < dictionary.Count; i++)
                {
                    string key = dictionary.ElementAt(i).Key;

                    double normalizedValue = normalizationMinimum + ((double)i / (dictionary.Count - 1)) * (normalizationMaximum - normalizationMinimum);

                    dictionary[key] = normalizedValue;
                }
            }

            double[][] normalizedData = new double[dataFromDatabase.Count][];

            for (int i = 0; i < dataFromDatabase.Count; i++)
            {
                normalizedData[i] = new double[dataFromDatabase[i].Length];
                for (int j = 0; j < dataFromDatabase[i].Length; j++)
                {
                    normalizedData[i][j] = dataDictionaries[j][dataFromDatabase[i][j]];
                }
                Console.WriteLine();
            }

            return new NormalizedDataAndDictionaries(normalizedData, dataDictionaries);
        }
    }
}