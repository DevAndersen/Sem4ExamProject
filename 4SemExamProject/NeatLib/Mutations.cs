using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib
{
    public static class Mutations
    {
        private delegate void MutationMethod(Ann ann);

        private static List<MutationMethod> mutationChances = new List<MutationMethod>()
        {
            MutateSynapseAddRandom,
            MutateSynapseWeight,
            MutateSynapseFromLayer,
            MutateSynapseFromNeuron,
            MutateSynapseToLayer,
            MutateSynapseToNeuron,
            MutateSynapseRemoveRandom,
            MutateNeuronAddRandom,
            MutateNeuronBias,
            MutateNeuronNeuronIdPush,
            MutateNeuronLayerRandomExisting,
            MutateNeuronLayerPush,
            MutateNeuronRemoveRandom,
        };

        public static void RollToCauseRandomMutation(Ann ann, int mutationRate, int mutationRolls)
        {
            for (int i = 0; i < mutationRolls; i++)
            {
                if (Util.rand.Next(mutationRate) == 0)
                {
                    mutationChances[Util.rand.Next(mutationChances.Count)].Invoke(ann);
                }
            }
        }

        #region Synapse mutations
        
        private static void MutateSynapseAddRandom(Ann ann)
        {
            if (ann.hiddenNeurons.Count == 0)
                return;

            int[] layers = ann.GetAllLayers();
            int fromLayer = layers.Length != 0 ? layers[Util.rand.Next(layers.Length)] : 0;
            int[] fromNeuronPositions = ann.GetNeuronsForLayer(fromLayer).Select(x => x.NeuronPosition).ToArray();
            int fromNeuron = fromNeuronPositions[Util.rand.Next(fromNeuronPositions.Length)];
            int toLayer = layers.Length != 0 ? layers[Util.rand.Next(layers.Length)] : 0;
            int[] toNeuronPositions = ann.GetNeuronsForLayer(toLayer).Select(x => x.NeuronPosition).ToArray();
            int toNeuron = toNeuronPositions[Util.rand.Next(toNeuronPositions.Length)];
            if(toLayer != fromLayer)
            {
                ann.synapses.Add(new Synapse(fromLayer, fromNeuron, toLayer, toNeuron));
            }
        }

        private static void MutateSynapseWeight(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            ann.synapses[Util.rand.Next(ann.synapses.Count)].Weight = Util.rand.NextDouble();
        }

        private static void MutateSynapseFromLayer(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            Synapse synapse = ann.synapses[Util.rand.Next(ann.synapses.Count)];
            int layer = ann.GetAllLayers()[Util.rand.Next(ann.GetAllLayers().Length)];

            if(synapse.ToLayer != layer)
            {
                synapse.FromLayer = layer;
            }
        }

        private static void MutateSynapseFromNeuron(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            Synapse synapse = ann.synapses[Util.rand.Next(ann.synapses.Count)];
            int fromLayer = synapse.FromLayer;
            Neuron[] neuronsInLayer = ann.GetNeuronsForLayer(fromLayer);
            Neuron neuron = neuronsInLayer.Length != 0 ? neuronsInLayer[Util.rand.Next(neuronsInLayer.Length)] : null;
            int newNeuronPosition = neuron != null ? neuron.NeuronPosition : 0;
            synapse.FromNeuron = newNeuronPosition;
        }

        private static void MutateSynapseToLayer(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            if (ann.synapses.Count == 0)
                return;

            Synapse synapse = ann.synapses[Util.rand.Next(ann.synapses.Count)];
            int layer = ann.GetAllLayers()[Util.rand.Next(ann.GetAllLayers().Length)];

            if (synapse.FromLayer != layer)
            {
                synapse.ToLayer = layer;
            }
        }

        private static void MutateSynapseToNeuron(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            Synapse synapse = ann.synapses[Util.rand.Next(ann.synapses.Count)];
            int toLayer = synapse.ToLayer;
            Neuron[] neuronsInLayer = ann.GetNeuronsForLayer(toLayer);
            Neuron neuron = neuronsInLayer.Length != 0 ? neuronsInLayer[Util.rand.Next(neuronsInLayer.Length)] : null;
            int newNeuronPosition = neuron != null ? neuron.NeuronPosition : 0;
            synapse.ToNeuron = newNeuronPosition;
        }

        private static void MutateSynapseRemoveRandom(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            ann.synapses.RemoveAt(Util.rand.Next(ann.synapses.Count));
        }

        #endregion

        #region Neuron mutations

        private static void MutateNeuronAddRandom(Ann ann)
        {
            int[] layers = ann.GetHiddenLayers();
            int layer = layers.Length != 0 ? layers[Util.rand.Next(layers.Length)] : 0;
            int[] neuronPositions = ann.GetNeuronsForLayer(layer).Select(x => x.NeuronPosition).ToArray();
            int neuronId = 0;
            bool keepSearching = true;
            while(keepSearching)
            {
                if(neuronPositions.Contains(neuronId))
                {
                    neuronId++;
                }
                else
                {
                    keepSearching = false;
                }
            }
            ann.hiddenNeurons.Add(new Neuron(layer, neuronId));
        }

        private static void MutateNeuronBias(Ann ann)
        {
            ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)].Bias = Util.rand.NextDouble();
        }

        private static void MutateNeuronNeuronIdPush(Ann ann)
        {
            if (ann.hiddenNeurons.Count == 0)
                return;

            bool working = true;
            while(working)
            {
                int push = Util.rand.Next(2) == 0 ? 1 : -1;
                Neuron neuron = ann.hiddenNeurons[Util.rand.Next(ann.hiddenNeurons.Count)];
                if (neuron.NeuronPosition + push >= 0)
                {
                    neuron.NeuronPosition += push;
                    working = false;
                }
            }
        }

        private static void MutateNeuronLayerRandomExisting(Ann ann)
        {
            if (ann.hiddenNeurons.Count == 0)
                return;

            int[] layers = ann.GetHiddenLayers();
            int randomLayer = layers[Util.rand.Next(layers.Length)];
            ann.hiddenNeurons[Util.rand.Next(ann.hiddenNeurons.Count)].Layer = randomLayer;
        }

        private static void MutateNeuronLayerPush(Ann ann)
        {
            if (ann.hiddenNeurons.Count == 0)
                return;

            bool working = true;
            while(working)
            {
                int push = Util.rand.Next(2) == 0 ? 1 : -1;
                Neuron neuron = ann.hiddenNeurons[Util.rand.Next(ann.hiddenNeurons.Count)];
                if(neuron.Layer + push >= 0)
                {
                    neuron.Layer += push;
                    working = false;
                }
            }
        }

        private static void MutateNeuronRemoveRandom(Ann ann)
        {
            if (ann.hiddenNeurons.Count == 0)
                return;

            ann.hiddenNeurons.RemoveAt(Util.rand.Next(ann.hiddenNeurons.Count));
        }

        #endregion
    }
}