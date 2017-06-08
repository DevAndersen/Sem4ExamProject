using DatabaseNormalizer.DatabaseHandlers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DatabaseNormalizer.NormalizedDataAndDictionaries;

namespace DatabaseNormalizer
{
    public class DataManager
    {
        public static NormalizedDataAndDictionaries GetNormalizedDataFromDatabase(IDatabaseHandler databaseHandler, string databaseLocation, string[] columns, string queryConditions, double normalizationMinimum, double normalizationMaximum, double numericNormalizationMargin)
        {
            List<string[]> dataFromDatabase = databaseHandler.GetDataFromDatabase(databaseLocation, columns, queryConditions);

            return NormalizeData(dataFromDatabase, columns, normalizationMinimum, normalizationMaximum, numericNormalizationMargin);
        }

        private static NormalizedDataAndDictionaries NormalizeData(List<string[]> dataFromDatabase, string[] columns, double normalizedFloor, double normalizedCeiling, double numericNormalizationMargin)
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
            
            List<DenormalizationVariables> denormalizationVariablesList = new List<DenormalizationVariables>();

            foreach (Dictionary<string, double> dictionary in dataDictionaries)
            {
                bool isNumeric = true;

                for (int i = 0; i < dictionary.Count; i++)
                {
                    //outValue only exists because it needs to, due to the nature of TryParse methods.
                    double outValue = 0;
                    bool isKeyNumeric = Double.TryParse(dictionary.ElementAt(i).Key, out outValue) || dictionary.ElementAt(i).Key.Length == 0;
                    if (!isKeyNumeric)
                    {
                        isNumeric = false;
                    }
                }

                if(isNumeric)
                {
                    double? smallestTrainingValue = null;
                    double? largestTrainingValue = null;

                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        string key = dictionary.ElementAt(i).Key;

                        double keyAsDouble = Double.TryParse(key.Length == 0 ? 0.ToString() : key, out keyAsDouble) ? keyAsDouble : throw new Exception($"Could not convert {key} to type Double.");

                        if (smallestTrainingValue == null || keyAsDouble < smallestTrainingValue)
                        {
                            smallestTrainingValue = keyAsDouble;
                        }
                        if (largestTrainingValue == null || keyAsDouble > largestTrainingValue)
                        {
                            largestTrainingValue = keyAsDouble;
                        }
                    }
                    denormalizationVariablesList.Add(new DenormalizationVariables(normalizedFloor, normalizedCeiling, numericNormalizationMargin, smallestTrainingValue.Value, largestTrainingValue.Value));

                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        string key = dictionary.ElementAt(i).Key;
                        double keyAsDouble = Double.TryParse(key.Length == 0 ? 0.ToString() : key, out keyAsDouble) ? keyAsDouble : throw new Exception($"Could not convert {key} to type Double.");
                        double normalizedValue = NormalizeNumeric(keyAsDouble, normalizedFloor, normalizedCeiling, numericNormalizationMargin, smallestTrainingValue.Value, largestTrainingValue.Value);
                        dictionary[key] = normalizedValue;
                    }
                }
                else
                {
                    denormalizationVariablesList.Add(null);
                    for (int i = 0; i < dictionary.Count; i++)
                    {
                        string key = dictionary.ElementAt(i).Key;
                        double normalizedValue = NormalizeIndex(i, dictionary.Count, normalizedFloor, normalizedCeiling);
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

            return new NormalizedDataAndDictionaries(normalizedData, dataDictionaries, denormalizationVariablesList);
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

        public static double DenormalizeNumeric(double value, double normalizedFloor, double normalizedCeiling, double normalizationMargin, double smallestTrainingValue, double largestTrainingValue)
        {
            double normSmall = (normalizedCeiling - normalizedFloor) * normalizationMargin;
            double normLarge = (normalizedCeiling - normalizedFloor) * (1 - normalizationMargin);
            return (value - normSmall) * ((largestTrainingValue - smallestTrainingValue) / (normLarge - normSmall));
        }
    }
}