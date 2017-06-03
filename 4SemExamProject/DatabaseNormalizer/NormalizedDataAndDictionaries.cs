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

        public NormalizedDataAndDictionaries(double[][] normalizedData, Dictionary<string, double>[] normalizationDictionaries)
        {
            NormalizedData = normalizedData;
            NormalizationDictionaries = normalizationDictionaries;
        }
    }
}