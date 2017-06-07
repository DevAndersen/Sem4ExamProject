using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeatLib2
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
            Neuron synapseNeuron = ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)];

            if (!HelperDoesSynapseExist(ann, fromLayer, fromNeuron))
            {
                synapseNeuron.synapses.Add(new Synapse(fromLayer, fromNeuron));
            }
        }

        private static void MutateSynapseWeightRandomize(Ann ann)
        {
            if (HelperCountSynapses(ann) == 0)
                return;

            Neuron synapseNeuron = ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)];

            if (synapseNeuron.synapses.Count == 0)
                return;

            Synapse synapse = synapseNeuron.synapses[Util.rand.Next(synapseNeuron.synapses.Count)];
            synapse.Weight = Util.rand.NextDouble();
        }

        private static void MutateSynapseWeightIncreaseRandom(Ann ann)
        {
            if (HelperCountSynapses(ann) == 0)
                return;

            Neuron synapseNeuron = ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)];

            if (synapseNeuron.synapses.Count == 0)
                return;

            Synapse synapse = synapseNeuron.synapses[Util.rand.Next(synapseNeuron.synapses.Count)];
            synapse.Weight += Util.rand.NextDouble();
        }

        private static void MutateSynapseWeightDecreaseRandom(Ann ann)
        {
            if (HelperCountSynapses(ann) == 0)
                return;

            Neuron synapseNeuron = ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)];

            if (synapseNeuron.synapses.Count == 0)
                return;

            Synapse synapse = synapseNeuron.synapses[Util.rand.Next(synapseNeuron.synapses.Count)];
            synapse.Weight -= Util.rand.NextDouble();
        }

        private static void MutateSynapseFromLayer(Ann ann)
        {
            if (HelperCountSynapses(ann) == 0)
                return;

            Neuron synapseNeuron = ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)];

            if (synapseNeuron.synapses.Count == 0)
                return;

            Synapse synapse = synapseNeuron.synapses[Util.rand.Next(synapseNeuron.synapses.Count)];
            int layer = ann.GetAllLayers()[Util.rand.Next(ann.GetAllLayers().Length)];

            if(!HelperDoesSynapseExist(ann, layer, synapse.FromNeuron))
            {
                synapse.FromLayer = layer;
            }
        }

        private static void MutateSynapseFromNeuron(Ann ann)
        {
            if (HelperCountSynapses(ann) == 0)
                return;

            Neuron synapseNeuron = ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)];

            if (synapseNeuron.synapses.Count == 0)
                return;

            Synapse synapse = synapseNeuron.synapses[Util.rand.Next(synapseNeuron.synapses.Count)];
            int fromLayer = synapse.FromLayer;
            Neuron[] neuronsInLayer = ann.GetNeuronsForLayer(fromLayer);
            Neuron neuron = neuronsInLayer.Length != 0 ? neuronsInLayer[Util.rand.Next(neuronsInLayer.Length)] : null;
            int newNeuronPosition = neuron != null ? neuron.NeuronPosition : 0;
            if (!HelperDoesSynapseExist(ann, synapse.FromLayer, newNeuronPosition))
            {
                synapse.FromNeuron = newNeuronPosition;
            }
        }
        
        private static void MutateSynapseRemoveRandom(Ann ann)
        {
            if (HelperCountSynapses(ann) == 0)
                return;

            Neuron neuron = ann.GetAllNeurons()[Util.rand.Next(ann.GetAllNeurons().Count)];

            if (neuron.synapses.Count == 0)
                return;

            neuron.synapses.RemoveAt(Util.rand.Next(neuron.synapses.Count));
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

        private static bool HelperDoesSynapseExist(Ann ann, int fromLayer, int fromNeuron)
        {
            bool exists = false;

            foreach (Neuron neuron in ann.GetAllNeurons())
            {
                foreach (Synapse synapse in neuron.synapses)
                {
                    if (synapse.FromLayer == fromLayer && synapse.FromNeuron == fromNeuron)
                    {
                        exists = true;
                    }
                }
            }

            return exists;
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
            List<Neuron> neuronCheckList = new List<Neuron>();

            foreach (Neuron neuron in ann.GetAllNeurons())
            {
                List<Synapse> synapseCheckList = new List<Synapse>();
                foreach (Synapse synapse in neuron.synapses)
                {
                    bool foundOnChecklist = false;
                    foreach (Synapse synapseFromCheckList in synapseCheckList)
                    {
                        if (synapse.FromLayer == synapseFromCheckList.FromLayer && synapse.FromNeuron == synapseFromCheckList.FromNeuron)
                        {
                            foundOnChecklist = true;
                        }
                    }

                    if (!foundOnChecklist)
                    {
                        synapseCheckList.Add(synapse);
                    }
                }
                neuron.synapses = synapseCheckList;
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
            ann.hiddenNeurons = neuronCheckList;
        }

        private static void HelperCleanUpBackwardsSynapses(Ann ann)
        {
            foreach (Neuron neuron in ann.GetAllNeurons())
            {
                List<Synapse> synapseChecklist = new List<Synapse>();
                foreach (Synapse synapse in neuron.synapses)
                {
                    if(synapse.FromLayer > neuron.Layer)
                    {
                        synapseChecklist.Add(synapse);
                    }
                }
                neuron.synapses = synapseChecklist;
            }
        }

        private static int HelperCountSynapses(Ann ann)
        {
            int synapseCount = 0;
            foreach (Neuron neuron in ann.GetAllNeurons())
            {
                foreach (Synapse synapse in neuron.synapses)
                {
                    synapseCount++;
                }
            }
            return synapseCount;
        }

        #endregion
    }
}