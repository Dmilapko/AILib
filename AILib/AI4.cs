using System;
using System.Collections.Generic;
using System.Runtime;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using MonoHelper;

namespace AILib
{
    public class AI4Neuron
    {
        public double value = 0.5;
        public List<double> weights;
        public double bias = 0;

        public object Clone()
        {
            return MemberwiseClone();
        }

        public AI4Neuron()
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

        public void RandBias(double diap)
        {
            double offset = 0;
            diap *= 2;
            if ((bias + diap / 2) > 1)
                offset = 1 - diap / 2 - bias;
            if ((bias - diap / 2) < -1)
                offset = -1 * bias + diap / 2 - 1;
            bias += (MHeleper.RandomDouble() - 0.5) * diap + offset;
            if (bias > 1) bias = 1;
            if (bias < -1) bias = -1;
        }

        public AI4Neuron(int weightscount)
        {
            weights = new List<double>(new double[weightscount]);
        }

        public AI4Neuron(AI4Neuron parent)
        {
            value = 0.5;
            weights = new List<double>(parent.weights);
        }
    }

    public class AI4
    {
        /// <summary>
        /// Input values to AI
        /// </summary>
        public List<AI4Neuron> inputs;
        /// <summary>
        /// Hidden  layer neurons, aka "Black magic box"
        /// </summary>
        public List<List<AI4Neuron>> neuronlayers = new List<List<AI4Neuron>>();
        /// <summary>
        /// Output(decisions) of AI
        /// </summary>
        public List<AI4Neuron> outputs;
        public double mygoodness = 0;
        public int generation = 0;
        public double wps = 0;

        public static double Clever_Sigmoid(double value)
        {
            value -= 0.5;
            return 1.0 / (1.0 + Math.Exp(-value));
        }

        public AI4()
        {

        }

        public void SaveToFile(string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(this));
        }

        public static AI4 ReadFromFile(string filename)
        {
            return JsonConvert.DeserializeObject<AI4>(File.ReadAllText(filename));
        }

        /// <summary>
        /// Random initialization
        /// </summary>
        /// <param name="Size_of_Layers">Size of layers(including input&output) of NEURONS </param>        
        public AI4(List<int> Size_of_Layers)
        {
            Size_of_Layers.Add(0);
            for (int i = 0; i < Size_of_Layers.Count - 1; i++)
            {
                neuronlayers.Add(new List<AI4Neuron>());
                for (int j = 0; j < Size_of_Layers[i]; j++)
                {
                    neuronlayers[i].Add(new AI4Neuron(Size_of_Layers[i + 1]));
                    neuronlayers[i][j].RandWeight(0);
                }
            }
            Fin_Init();
        }

        /// <summary>
        /// Generates new AI4 from PARENT AI4
        /// </summary>
        /// <param name="parent">Parent AI4</param>
        /// <param name="goodness">how good PARENT was [0; 1]</param>
        public AI4(AI4 parent, double goodness, bool straight = true)
        {
            generation = parent.generation;
            //if (straight) goodness = Math.Sqrt(goodness);
            if (straight) goodness = Math.Pow(goodness, 1 / (double)(parent.neuronlayers.Count));
            for (int i = 0; i < parent.neuronlayers.Count; i++)
            {
                neuronlayers.Add(new List<AI4Neuron>());
                for (int j = 0; j < parent.neuronlayers[i].Count; j++)
                {
                    neuronlayers[i].Add(new AI4Neuron(parent.neuronlayers[i][j]));
                    neuronlayers[i][j].RandWeight(goodness);
                }
            }
            Fin_Init();
        }

        private void Fin_Init()
        {
            inputs = new List<AI4Neuron>(neuronlayers[0]);
            outputs = new List<AI4Neuron>(neuronlayers[neuronlayers.Count - 1]);
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
            for (int i = 1; i < neuronlayers.Count; i++)
            {
                for (int j = 0; j < neuronlayers[i].Count; j++)
                {
                    for (int c = 0; c < neuronlayers[i - 1].Count; c++)
                    {
                        neuronlayers[i][j].value += neuronlayers[i][j].bias;
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
