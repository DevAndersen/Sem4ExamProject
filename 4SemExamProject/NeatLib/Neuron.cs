using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib
{
    [Serializable]
    public class Neuron
    {
        public int Layer { get; set; }
        public int NeuronPosition { get; set; }
        public double Bias { get; set; }
        public double Value { get; set; }

        /// <summary>
        /// Initialize a neuron for use in the input or output layer.
        /// </summary>
        public Neuron(BaseNeuronType baseNeuronType, int neuronPosition)
        {
            Layer = (int)baseNeuronType;
            NeuronPosition = neuronPosition;
        }

        /// <summary>
        /// Initialize a neuron for a specific hidden layer
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="neuronPosition"></param>
        public Neuron(int layer, int neuronPosition)
        {
            Layer = layer;
            NeuronPosition = neuronPosition;
        }

        public enum BaseNeuronType
        {
            Input = -1,
            Output = -2
        }
    }
}