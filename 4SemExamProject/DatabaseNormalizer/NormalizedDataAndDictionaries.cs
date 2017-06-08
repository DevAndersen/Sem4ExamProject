using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseNormalizer
{
    public class NormalizedDataAndDictionaries
    {
        public double[][] NormalizedData { get; }
        public Dictionary<string, double>[] NormalizationDictionaries { get; }
        public List<DenormalizationVariables> denormalizationVariablesList = new List<DenormalizationVariables>();

        public NormalizedDataAndDictionaries(double[][] normalizedData, Dictionary<string, double>[] normalizationDictionaries, List<DenormalizationVariables> denormalizationVariablesList)
        {
            NormalizedData = normalizedData;
            NormalizationDictionaries = normalizationDictionaries;
            this.denormalizationVariablesList = denormalizationVariablesList;
        }

        public class DenormalizationVariables
        {
            public double NormalizedFloor { get; set; }
            public double NormalizedCeiling { get; set; }
            public double NumericNormalizationMargin { get; set; }
            public double SmallestTrainingValue { get; set; }
            public double LargestTrainingValue { get; set; }

            public DenormalizationVariables(double normalizedFloor, double normalizedCeiling, double numericNormalizationMargin, double smallestTrainingValue, double largestTrainingValue)
            {
                NormalizedFloor = normalizedFloor;
                NormalizedCeiling = normalizedCeiling;
                NumericNormalizationMargin = numericNormalizationMargin;
                SmallestTrainingValue = smallestTrainingValue;
                LargestTrainingValue = largestTrainingValue;
            }
        }
    }
}