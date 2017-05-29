using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib
{
    [Serializable]
    public class Synapse
    {
        public int FromLayer { get; set; }
        public int FromNeuron { get; set; }
        public int ToLayer { get; set; }
        public int ToNeuron { get; set; }
        public double Weight { get; set; }

        public Synapse(int fromLayer, int fromNeuron, int toLayer, int toNeuron)
        {
            FromLayer = fromLayer;
            FromNeuron = fromNeuron;
            ToLayer = toLayer;
            ToNeuron = toNeuron;

            Weight = Util.rand.NextDouble();
        }
    }
}