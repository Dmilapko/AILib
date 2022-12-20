using System;
using System.Collections.Generic;
using System.Runtime;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using MonoHelper;

namespace AILib
{
    public class Clever_Neuron
    {
        public double value = 0.5;
        public List<double> weights;

        public object Clone()
        {
            return MemberwiseClone();
        }

        public Clever_Neuron()
        {

        }

        public void RandWeight(double goodness)
        {
            double badness = (1 - goodness);
            for (int i = 0; i < weights.Count; i++)
            {
                double offset = 0;
                double diapason = Math.Abs(2 * Math.Pow(badness, 1));
                if ((weights[i] + diapason / 2) > 1)
                    offset = 1 - diapason / 2 - weights[i];
                if ((weights[i] - diapason / 2) < -1)
                    offset = -1 * weights[i] + diapason / 2 - 1;
                weights[i] += (MHeleper.RandomDouble() - 0.5) * diapason + offset;
                if (weights[i] > 1) weights[i] = 1;
                if (weights[i] < -1) weights[i] = -1;
            }
        }

        public Clever_Neuron(int weightscount)
        {
            weights = new List<double>(new double[weightscount]);
        }

        public Clever_Neuron(Clever_Neuron parent)
        {
            value = 0.5;
            weights = new List<double>(parent.weights);
        }
    }

    public class Clever_AI
    {
        /// <summary>
        /// Input values to AI
        /// </summary>
        public List<Clever_Neuron> inputs;
        /// <summary>
        /// Hidden  layer neurons, aka "Black magic box"
        /// </summary>
        public List<List<Clever_Neuron>> neuronlayers = new List<List<Clever_Neuron>>();
        /// <summary>
        /// Output(decisions) of AI
        /// </summary>
        public List<Clever_Neuron> outputs;
        public double mygoodness = 0;
        public int generation = 0;
        public double wps = 0;
        /// <summary>
        /// Biases of AI. Applyes only to inputs
        /// </summary>
        public List<double> biases = new List<double>();

        public static double Clever_Sigmoid(double value)
        {
            value -= 0.5;
            return 1.0 / (1.0 + Math.Exp(-value));
        }

        private double RandBias(double bias, double n_goodness)
        {
            double badness = (1 - n_goodness);
            double diapason = 2 * Math.Pow(badness, 1) * 0;
            double offset = 0;
            if ((bias + diapason / 2) > 1)
                offset = 1 - diapason / 2 - bias;
            if ((bias - diapason / 2) < -1)
                offset = -1 * bias + diapason / 2 - 1;
            bias += (MHeleper.RandomDouble() - 0.5) * diapason + offset;
            if (bias > 1) bias = 1;
            if (bias < -1) bias = -1;
            return bias;
        }

        public Clever_AI()
        {

        }

        public void SaveToFile(string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(this));
        }

        public static Clever_AI ReadFromFile(string filename)
        {
            return JsonConvert.DeserializeObject<Clever_AI>(File.ReadAllText(filename));
        }

        /// <summary>
        /// Random initialization
        /// </summary>
        /// <param name="Size_of_Layers">Size of layers(including input&output) of NEURONS </param>        
        public Clever_AI(List<int> Size_of_Layers)
        {
            Size_of_Layers.Add(0);
            for (int i = 0; i < Size_of_Layers.Count - 1; i++)
            {
                neuronlayers.Add(new List<Clever_Neuron>());
                for (int j = 0; j < Size_of_Layers[i]; j++)
                {
                    neuronlayers[i].Add(new Clever_Neuron(Size_of_Layers[i + 1]));
                    neuronlayers[i][j].RandWeight(0);
                }
            }
            for (int i = 0; i < Size_of_Layers[0]; i++)
            {
                biases.Add(RandBias(0, 0));
            }
            Fin_Init();
        }

        /// <summary>
        /// Generates new Clever_AI from PARENT Clever_AI
        /// </summary>
        /// <param name="parent">Parent Clever_AI</param>
        /// <param name="goodness">how good PARENT was [0; 1]</param>
        public Clever_AI(Clever_AI parent, double goodness, bool straight = true)
        {
            generation = parent.generation;
            //if (straight) goodness = Math.Sqrt(goodness);
            if (straight) goodness = Math.Pow(Math.Sqrt(goodness), 1/(double)(parent.neuronlayers.Count - 1));
            foreach (var bias in parent.biases)
            {
                biases.Add(RandBias(bias, goodness));
            }
            for (int i = 0; i < parent.neuronlayers.Count; i++)
            {
                neuronlayers.Add(new List<Clever_Neuron>());
                for (int j = 0; j < parent.neuronlayers[i].Count; j++)
                {
                    neuronlayers[i].Add(new Clever_Neuron(parent.neuronlayers[i][j]));
                    neuronlayers[i][j].RandWeight(goodness);
                }
            }
            Fin_Init();
        }

        private void Fin_Init()
        {
            inputs = new List<Clever_Neuron>(neuronlayers[0]);
            outputs = new List<Clever_Neuron>(neuronlayers[neuronlayers.Count - 1]);
            foreach (var list in neuronlayers)
            {
                foreach (var neuron in list)
                {
                    neuron.value = 0.5;
                }
            }
        }

        public void Run()
        {
            for (int i = 0; i < inputs.Count; i++)
            {
             //   inputs[i].value += biases[i];
                inputs[i].value += -0.5;
            }
            for (int i = 1; i < neuronlayers.Count; i++)
            {
                for (int j = 0; j < neuronlayers[i].Count; j++)
                {
                    for (int c = 0; c < neuronlayers[i - 1].Count; c++)
                    {
                        neuronlayers[i][j].value += neuronlayers[i - 1][c].weights[j] * neuronlayers[i - 1][c].value;
                    }
                    neuronlayers[i][j].value = Clever_Sigmoid(neuronlayers[i][j].value);
                }
            }
            outputs = neuronlayers[neuronlayers.Count - 1];
        }

        /*    public void Run()
            {
                for (int i = 1; i < neuronlayers.Count; i++)
                {
                    for (int j = 0; j < neuronlayers[i].Count; j++)
                    {
                        neuronlayers[i][j].value = neuronlayers[i][j].bias;
                        for (int c = 0; c < neuronlayers[i - 1].Count; c++)
                        {
                            neuronlayers[i][j].value += neuronlayers[i - 1][c].weights[j] * neuronlayers[i - 1][c].value;
                        }
                        if (compress_res || (i != (neuronlayers.Count - 1)))
                            neuronlayers[i][j].value = Clever_Sigmoid(neuronlayers[i][j].value);
                    }
                }
                outputs = neuronlayers[neuronlayers.Count - 1];
            }*/

        private int cnt = 0;

        private void Fast_M(int layer, int st, int fn)
        {
            for (int j = st; j <= fn; j++)
            {
                for (int c = 0; c < neuronlayers[layer - 1].Count; c++)
                {
                    neuronlayers[layer][j].value += neuronlayers[layer - 1][c].weights[j] * neuronlayers[layer - 1][c].value;
                }
                neuronlayers[layer][j].value = Clever_Sigmoid(neuronlayers[layer][j].value);
            }
            cnt++;
        }

        /// <summary>
        /// VERY DANGEROUS fuction to use. Uses multithreading to increase perfomance
        /// </summary>
        public void Run_FAST()
        {
            wps = 0;
            for (int i = 1; i < neuronlayers.Count; i++)
            {
                cnt = 0;
                int prev = -1;
                int step = neuronlayers[i].Count / Environment.ProcessorCount;
                for (int c = 0; c < Environment.ProcessorCount - 1; c++)
                {
                    var t = new Thread(() => Fast_M(i, prev + 1, prev + step));
                    t.Priority = ThreadPriority.Highest;
                    prev += step;
                    t.Start();
                }
                for (int j = prev + 1; j < neuronlayers[i].Count; j++)
                {
                    for (int c = 0; c < neuronlayers[i - 1].Count; c++)
                    {
                        neuronlayers[i][j].value += neuronlayers[i - 1][c].weights[j] * neuronlayers[i - 1][c].value;
                    }
                    neuronlayers[i][j].value = Clever_Sigmoid(neuronlayers[i][j].value);
                }
                DateTime now = DateTime.Now;
                while (cnt != Environment.ProcessorCount - 1)
                {

                }
                wps = (DateTime.Now - now).TotalMilliseconds;
            }
            outputs = neuronlayers[neuronlayers.Count - 1];
        }
    }
}
