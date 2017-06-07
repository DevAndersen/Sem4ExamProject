using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib2
{
    [Serializable]
    public class Synapse
    {
        public int FromLayer { get; set; }
        public int FromNeuron { get; set; }
        public double Weight { get; set; }

        public Synapse(int fromLayer, int fromNeuron)
        {
            FromLayer = fromLayer;
            FromNeuron = fromNeuron;

            Weight = Util.rand.NextDouble();
        }
    }
}