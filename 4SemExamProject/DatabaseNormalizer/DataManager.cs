using DatabaseNormalizer.DatabaseHandlers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseNormalizer
{
    public class DataManager
    {
        public static NormalizedDataAndDictionaries GetNormalizedDataFromDatabase(IDatabaseHandler databaseHandler, string databaseLocation, string[] columns, string queryConditions, double normalizationMinimum, double normalizationMaximum)
        {
            List<string[]> dataFromDatabase = databaseHandler.GetDataFromDatabase(databaseLocation, columns, queryConditions);

            return NormalizeData(dataFromDatabase, columns, normalizationMinimum, normalizationMaximum);
        }

        private static NormalizedDataAndDictionaries NormalizeData(List<string[]> dataFromDatabase, string[] columns, double normalizationMinimum, double normalizationMaximum)
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
                bool isNumeric = true;

                for (int i = 0; i < dictionary.Count; i++)
                {
                    double value = 0;
                    bool isKeyNumeric = Double.TryParse(dictionary.ElementAt(i).Key, out value);
                    if (!isKeyNumeric)
                    {
                        isNumeric = false;
                    }
                }

                if(isNumeric)
                {
                    double? smallestTrainingValue = null;
                    double? largestTrainingValue = null;
                    double margin = 0.25;

                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        string key = dictionary.ElementAt(i).Key;

                        double keyAsDouble = Double.TryParse(key, out keyAsDouble) ? keyAsDouble : throw new Exception($"Could not convert {key} to type Double.");

                        if (smallestTrainingValue == null || keyAsDouble < smallestTrainingValue)
                        {
                            smallestTrainingValue = keyAsDouble;
                        }
                        if (largestTrainingValue == null || keyAsDouble > largestTrainingValue)
                        {
                            largestTrainingValue = keyAsDouble;
                        }
                    }

                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        string key = dictionary.ElementAt(i).Key;

                        double keyAsDouble = Double.TryParse(key, out keyAsDouble) ? keyAsDouble : throw new Exception($"Could not convert {key} to type Double.");

                        double normalizedValue = NormalizeNumeric(keyAsDouble, normalizationMinimum, normalizationMaximum, margin, smallestTrainingValue.Value, largestTrainingValue.Value);

                        dictionary[key] = normalizedValue;
                    }
                }
                else
                {
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        string key = dictionary.ElementAt(i).Key;

                        double normalizedValue = NormalizeIndex(i, dictionary.Count, normalizationMinimum, normalizationMaximum);

                        dictionary[key] = normalizedValue;
                    }
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
            }

            return new NormalizedDataAndDictionaries(normalizedData, dataDictionaries);
        }

        private static double NormalizeIndex(int index, int length, double normalizationMinimum, double normalizationMaximum)
        {
            return normalizationMinimum + ((double)index / (length - 1)) * (normalizationMaximum - normalizationMinimum);
        }

        private static double NormalizeNumeric(double value, double normalizedFloor, double normalizedCeiling, double normalizationMargin, double smallestTrainingValue, double largestTrainingValue)
        {
            double normSmall = (normalizedCeiling - normalizedFloor) * normalizationMargin;
            double normLarge = (normalizedCeiling - normalizedFloor) * (1 - normalizationMargin);
            return normSmall + (value / (largestTrainingValue - smallestTrainingValue) * (normLarge - normSmall));
        }
    }
}