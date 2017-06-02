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
            MutateSynapseWeightRandomize,
            MutateSynapseWeightIncreaseRandom,
            MutateSynapseWeightDecreaseRandom,
            MutateSynapseFromLayer,
            MutateSynapseFromNeuron,
            MutateSynapseToLayer,
            MutateSynapseToNeuron,
            MutateSynapseRemoveRandom,
            MutateNeuronAddRandom,
            MutateNeuronBiasRandomize,
            MutateNeuronBiasIncreaseRandom,
            MutateNeuronBiasDecreaseRandom,
            MutateNeuronNeuronIdPush,
            MutateNeuronLayerRandomExisting,
            MutateNeuronLayerPush,
            MutateNeuronRemoveRandom
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
            HelperCleanUpDuplicates(ann);
            HelperCleanUpBackwardsSynapses(ann);
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

            if(toLayer != fromLayer && !HelperDoesSynapseExist(ann, fromLayer, fromNeuron, toLayer, toNeuron))
            {
                ann.synapses.Add(new Synapse(fromLayer, fromNeuron, toLayer, toNeuron));
            }
        }

        private static void MutateSynapseWeightRandomize(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            ann.synapses[Util.rand.Next(ann.synapses.Count)].Weight = Util.rand.NextDouble() * 100;
        }

        private static void MutateSynapseWeightIncreaseRandom(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            ann.synapses[Util.rand.Next(ann.synapses.Count)].Weight += Util.rand.NextDouble() * 100;
        }

        private static void MutateSynapseWeightDecreaseRandom(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            ann.synapses[Util.rand.Next(ann.synapses.Count)].Weight -= Util.rand.NextDouble();
        }

        private static void MutateSynapseFromLayer(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            Synapse synapse = ann.synapses[Util.rand.Next(ann.synapses.Count)];
            int layer = ann.GetAllLayers()[Util.rand.Next(ann.GetAllLayers().Length)];

            if(synapse.ToLayer != layer && !HelperDoesSynapseExist(ann, layer, synapse.FromNeuron, synapse.ToLayer, synapse.ToNeuron))
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
            if (!HelperDoesSynapseExist(ann, synapse.FromLayer, newNeuronPosition, synapse.ToLayer, synapse.ToNeuron))
            {
                synapse.FromNeuron = newNeuronPosition;
            }
        }

        private static void MutateSynapseToLayer(Ann ann)
        {
            if (ann.synapses.Count == 0)
                return;

            if (ann.synapses.Count == 0)
                return;

            Synapse synapse = ann.synapses[Util.rand.Next(ann.synapses.Count)];
            int layer = ann.GetAllLayers()[Util.rand.Next(ann.GetAllLayers().Length)];

            if (synapse.FromLayer != layer && !HelperDoesSynapseExist(ann, synapse.FromLayer, synapse.FromNeuron, synapse.ToLayer, layer))
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
            if (!HelperDoesSynapseExist(ann, synapse.FromLayer, synapse.FromNeuron, synapse.ToLayer, newNeuronPosition))
            {
                synapse.ToNeuron = newNeuronPosition;
            }
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

            if(!HelperDoesNeuronExist(ann, layer, neuronId))
            {
                ann.hiddenNeurons.Add(new Neuron(layer, neuronId));
            }
        }

        private static void MutateNeuronBiasRandomize(Ann ann)
        {
            ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)].Bias = Util.rand.NextDouble();
        }

        private static void MutateNeuronBiasIncreaseRandom(Ann ann)
        {
            ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)].Bias += Util.rand.NextDouble() * 100;
        }

        private static void MutateNeuronBiasDecreaseRandom(Ann ann)
        {
            ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)].Bias -= Util.rand.NextDouble() * 100;
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
                if (neuron.NeuronPosition + push >= 0 && !HelperDoesNeuronExist(ann, neuron.Layer, neuron.NeuronPosition + push))
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
            Neuron neuron = ann.hiddenNeurons[Util.rand.Next(ann.hiddenNeurons.Count)];
            if(!HelperDoesNeuronExist(ann, randomLayer, neuron.NeuronPosition))
            {
                neuron.Layer = randomLayer;
            }
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
                if(neuron.Layer + push >= 0 && !HelperDoesNeuronExist(ann, neuron.Layer + push, neuron.NeuronPosition))
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

        #region Helper methods

        private static bool HelperDoesSynapseExist(Ann ann, int fromLayer, int fromNeuron, int toLayer, int toNeuron)
        {
            bool exists = ann.synapses.Where(x =>
            x.FromLayer == fromLayer &&
            x.FromNeuron == fromNeuron &&
            x.ToLayer == toLayer &&
            x.ToNeuron == toNeuron
            ).ToArray().Length != 0;

            bool existsReversed = ann.synapses.Where(x =>
            x.FromLayer == toLayer &&
            x.FromNeuron == toNeuron &&
            x.ToLayer == fromLayer &&
            x.ToNeuron == fromNeuron
            ).ToArray().Length != 0;

            return exists || existsReversed;
        }

        private static bool HelperDoesNeuronExist(Ann ann, int layer, int neuronPosition)
        {
            return ann.hiddenNeurons.Where(x =>
            x.Layer == layer &&
            x.NeuronPosition == neuronPosition
            ).ToArray().Length != 0;
        }

        private static void HelperCleanUpDuplicates(Ann ann)
        {
            //Synapse[] shuffledSynapses = ann.synapses.OrderBy(elem => Guid.NewGuid()).ToArray();
            //Neuron[] shuffledNeurons = ann.hiddenNeurons.OrderBy(elem => Guid.NewGuid()).ToArray();

            //throw new NotImplementedException("Remove duplicate synapses and neurons.");

            List<Synapse> synapseCheckList = new List<Synapse>();
            List<Neuron> neuronCheckList = new List<Neuron>();

            foreach (Synapse synapse in ann.synapses)
            {
                bool foundOnChecklist = false;
                foreach (Synapse synapseFromCheckList in synapseCheckList)
                {
                    if(synapse.FromLayer == synapseFromCheckList.FromLayer &&
                       synapse.FromNeuron == synapseFromCheckList.FromNeuron &&
                       synapse.ToLayer == synapseFromCheckList.ToLayer &&
                       synapse.ToNeuron == synapseFromCheckList.ToNeuron)
                    {
                        foundOnChecklist = true;
                    }
                }

                if(!foundOnChecklist)
                {
                    synapseCheckList.Add(synapse);
                }
            }

            foreach (Neuron neuron in ann.hiddenNeurons)
            {
                bool foundOnChecklist = false;
                foreach (Neuron neuronFromCheckList in neuronCheckList)
                {
                    if (neuron.Layer == neuronFromCheckList.Layer &&
                        neuron.NeuronPosition == neuronFromCheckList.NeuronPosition)
                    {
                        foundOnChecklist = true;
                    }
                }

                if (!foundOnChecklist)
                {
                    neuronCheckList.Add(neuron);
                }
            }

            ann.synapses = synapseCheckList;
            ann.hiddenNeurons = neuronCheckList;
        }

        private static void HelperCleanUpBackwardsSynapses(Ann ann)
        {
            List<Synapse> synapseChecklist = new List<Synapse>();

            foreach (Synapse synapse in ann.synapses)
            {
                if(synapse.FromLayer != -2 && (synapse.FromLayer == -1 || synapse.FromLayer < synapse.ToLayer))
                {
                    synapseChecklist.Add(synapse);
                }
            }

            ann.synapses = synapseChecklist;
        }

        #endregion
    }
}