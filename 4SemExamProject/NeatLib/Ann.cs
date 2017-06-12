using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib
{
    [Serializable]
    public class Ann
    {
        public double Error { get; set; } = -1;
        public int Generation { get; set; } = -1;

        public Neuron[] inputNeurons;
        public Neuron[] outputNeurons;
        public List<Neuron> hiddenNeurons = new List<Neuron>();
        public List<Synapse> synapses = new List<Synapse>();

        private ActivationFunction.ActivationMethod activationMethod;

        public Ann(int inputs, int outputs, ActivationFunction.ActivationMethod activationMethod)
        {
            if (inputs < 1 || outputs < 1)
                throw new ArgumentOutOfRangeException("There must be at least one input and one output.");

            this.activationMethod = activationMethod;

            InitNeurons(inputs, outputs);
            InitSynapses();
        }

        public double[] Execute(double[] inputs)
        {
            if (inputs.Length != inputNeurons.Length)
                throw new ArgumentOutOfRangeException("The number of inputs must match the number of input neurons.");

            foreach (Neuron neuron in GetAllNeurons())
            {
                neuron.Value = 0;
            }

            for (int i = 0; i < inputs.Length; i++)
            {
                inputNeurons[i].Value = inputs[i];
            }

            List<int> layersToGoThrough = new List<int>();
            layersToGoThrough.AddRange(GetHiddenLayers());
            layersToGoThrough.Add((int)Neuron.IONeuronType.Output);

            foreach (int layer in layersToGoThrough)
            {
                foreach (Neuron neuron in GetNeuronsForLayer(layer))
                {
                    Synapse[] synapsesToNeuron = synapses.Where(x => x.ToLayer == layer && x.ToNeuron == neuron.NeuronPosition).ToArray();
                    double value = 0;

                    foreach (Synapse synapseFromneuron in synapsesToNeuron)
                    {
                        Neuron[] connectedNeurons = GetAllNeurons().Where(y => y.Layer == synapseFromneuron.FromLayer && y.NeuronPosition == synapseFromneuron.FromNeuron).ToArray();
                        foreach (Neuron connectedNeuron in connectedNeurons)
                        {
                            value += connectedNeuron.Value * synapseFromneuron.Weight;
                        }
                    }
                    
                    neuron.Value = activationMethod.Invoke(value + neuron.Bias);
                }
            }

            double[] outputs = new double[outputNeurons.Length];
            for (int i = 0; i < outputNeurons.Length; i++)
            {
                outputs[i] = outputNeurons[i].Value;
            }
            return outputs;
        }

        private void InitNeurons(int inputs, int outputs)
        {
            inputNeurons = new Neuron[inputs];
            outputNeurons = new Neuron[outputs];

            for (int i = 0; i < inputNeurons.Length; i++)
            {
                inputNeurons[i] = new Neuron(Neuron.IONeuronType.Input, i);
            }
            for (int i = 0; i < outputNeurons.Length; i++)
            {
                outputNeurons[i] = new Neuron(Neuron.IONeuronType.Output, i);
            }
        }

        private void InitSynapses()
        {
            foreach (Neuron inputNeuron in inputNeurons)
            {
                foreach (Neuron outputNeuron in outputNeurons)
                {
                    synapses.Add(new Synapse(inputNeuron.Layer, inputNeuron.NeuronPosition, outputNeuron.Layer, outputNeuron.NeuronPosition));
                }
            }
        }

        public List<Neuron> GetAllNeurons()
        {
            List<Neuron> allNeurons = new List<Neuron>();
            allNeurons.AddRange(inputNeurons);
            allNeurons.AddRange(hiddenNeurons);
            allNeurons.AddRange(outputNeurons);
            return allNeurons;
        }

        public Dictionary<int, List<Neuron>> GetHiddenNeuronLayers()
        {
            Dictionary<int, List<Neuron>> layers = new Dictionary<int, List<Neuron>>();

            foreach (Neuron neuron in hiddenNeurons)
            {
                if (!layers.ContainsKey(neuron.Layer))
                {
                    layers.Add(neuron.Layer, new List<Neuron>() { neuron });
                }
                if (layers.ContainsKey(neuron.Layer))
                {
                    layers[neuron.Layer].Add(neuron);
                }
            }
            return layers;
        }

        public int[] GetHiddenLayers()
        {
            return GetHiddenNeuronLayers().Keys.ToArray();
        }
        
        public Dictionary<int, List<Neuron>> GetAllNeuronLayers()
        {
            Dictionary<int, List<Neuron>> layers = new Dictionary<int, List<Neuron>>();

            foreach (Neuron neuron in GetAllNeurons())
            {
                if (!layers.ContainsKey(neuron.Layer))
                {
                    layers.Add(neuron.Layer, new List<Neuron>() { neuron });
                }
                else if (layers.ContainsKey(neuron.Layer))
                {
                    layers[neuron.Layer].Add(neuron);
                }
            }
            return layers;
        }

        public int[] GetAllLayers()
        {
            return GetAllNeuronLayers().Keys.ToArray();
        }

        public Neuron[] GetNeuronsForLayer(int layer)
        {
            Dictionary<int, List<Neuron>> neurons = GetAllNeuronLayers();
            if(neurons.ContainsKey(layer))
            {
                return neurons[layer].Select(x => x).ToArray();
            }
            return new Neuron[0];
        }

        public List<Synapse> GetAllSynapses()
        {
            return synapses;
        }
    }
}