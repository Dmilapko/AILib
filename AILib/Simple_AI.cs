using System;
using System.Collections.Generic;
using System.Runtime;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using MonoHelper;

namespace AILib
{
    public class Simple_AI : ICloneable
    {
        /// <summary>
        /// Input values to Simple_AI
        /// </summary>
        public List<Neuron> inputs;
        /// <summary>
        /// Hidden  layer neurons, aka "Black magic box"
        /// </summary>
        public List<List<Neuron>> neuronlayers = new List<List<Neuron>>();
        /// <summary>
        /// Output(decisions) of Simple_AI
        /// </summary>
        public List<Neuron> outputs;
        public double mygoodness = 0;
        public int generation = 0;
        public double wps = 0;
        public bool compress_res = true;

        public object Clone()
        {
            return MemberwiseClone();
        }

        public static double Sigmoid(double value)
        {
            return 1.0 / (1.0 + Math.Exp(-value));
        }

        public static List<double> Compress(List<double> res)
        {
            for (int i = 0; i < res.Count; i++)
            {
                res[i] = Sigmoid(res[i]);
            }
            return res;
        }

        public Simple_AI()
        {

        }

        /// <summary>
        /// Random initialization
        /// </summary>
        /// <param name="Size_of_Layers">Size of layers(including input&output) of NEURONS </param>        
        public Simple_AI(List<int> Size_of_Layers)
        {
            Size_of_Layers.Insert(0, 0);
            Size_of_Layers.Add(0);
            neuronlayers = new List<List<Neuron>>();
            for (int i = 1; i < Size_of_Layers.Count - 1; i++)
            {
                neuronlayers.Add(new List<Neuron>());
                neuronlayers[i - 1] = new List<Neuron>(Size_of_Layers[i]);
                for (int j = 0; j < Size_of_Layers[i]; j++)
                {
                    neuronlayers[i - 1].Add(new Neuron(Size_of_Layers[i + 1], Size_of_Layers[i - 1]));
                    neuronlayers[i - 1][j].RandBias(0);
                    neuronlayers[i - 1][j].RandWeight(0);
                    neuronlayers[i - 1][j].value = 0.5;
                }
            }
            inputs = new List<Neuron>(neuronlayers[0]);
            outputs = new List<Neuron>(neuronlayers[neuronlayers.Count - 1]);
        }

        /// <summary>
        /// Generates new Simple_AI from PARENT Simple_AI
        /// </summary>
        /// <param name="parent">Parent Simple_AI</param>
        /// <param name="goodness">how good PARENT was [0; 1]</param>
        public Simple_AI(Simple_AI parent, double goodness, bool straight = true)
        {
            generation = parent.generation;
            if (straight) goodness = Math.Sqrt(goodness);
            //neuronlayers = parent.neuronlayers;

            for (int i = 0; i < parent.neuronlayers.Count; i++)
            {
                neuronlayers.Add(new List<Neuron>());
                for (int j = 0; j < parent.neuronlayers[i].Count; j++)
                {
                    neuronlayers[i].Add(new Neuron(parent.neuronlayers[i][j]));
                    neuronlayers[i][j].RandBias(goodness);
                    neuronlayers[i][j].RandWeight(goodness);
                }
            }
            inputs = new List<Neuron>(neuronlayers[0]);
            outputs = new List<Neuron>(neuronlayers[neuronlayers.Count - 1]);
        }

        public static bool ReadSaveAI(ref Simple_AI nextgenAI, string filename)
        {
            bool ok = false;
            if (File.Exists(filename))
            {
                if (File.ReadAllText(filename) != "")
                {
                    Simple_AI fileAI = Simple_AI.ReadFromFile(filename);
                    ok = true;
                    if (fileAI.mygoodness > nextgenAI.mygoodness)
                    {
                        nextgenAI = fileAI;
                    }
                    else
                    {
                        nextgenAI.SaveToFile(filename);
                    }
                }
            }
            if (!ok)
            {
                nextgenAI.SaveToFile(filename);
            }
            return ok;
        }

        public static bool ReadAI(ref Simple_AI nextgenAI, string filename)
        {
            bool ok = false;
            if (File.Exists(filename))
            {
                if (File.ReadAllText(filename) != "")
                {
                    Simple_AI fileAI = Simple_AI.ReadFromFile(filename);
                    ok = true;
                    if (fileAI.mygoodness > nextgenAI.mygoodness)
                    {
                        nextgenAI = fileAI;
                    }
                    else
                    {
                        nextgenAI.SaveToFile(filename);
                    }
                }
            }
            return ok;
        }

        public void SaveToFile(string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(this));
        }

        public static Simple_AI ReadFromFile(string filename)
        {
            return JsonConvert.DeserializeObject<Simple_AI>(File.ReadAllText(filename));
        }

        /*public void Run()
        {
            for (int i = 1; i < neuronlayers.Count; i++)
            {
                for (int j = 0; j < neuronlayers[i].Count; j++)
                {
                    for (int c = 0; c < neuronlayers[i-1].Count; c++)
                    {
                        neuronlayers[i][j].value += neuronlayers[i - 1][c].weights[j] * neuronlayers[i - 1][c].value;
                    }
                    if (compress_res || (i != (neuronlayers.Count - 1))) 
                        neuronlayers[i][j].value = Clever_Sigmoid(neuronlayers[i][j].value);
                }
            }
            outputs = neuronlayers[neuronlayers.Count - 1];
        }*/

        public void Run()
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
                        neuronlayers[i][j].value = Sigmoid(neuronlayers[i][j].value);
                }
            }
            outputs = neuronlayers[neuronlayers.Count - 1];
        }

        private int cnt = 0;

        private void Fast_M(int layer, int st, int fn)
        {
            for (int j = st; j <= fn; j++)
            {
                for (int c = 0; c < neuronlayers[layer - 1].Count; c++)
                {
                    neuronlayers[layer][j].value += neuronlayers[layer - 1][c].weights[j] * neuronlayers[layer - 1][c].value;
                }
                neuronlayers[layer][j].value = Sigmoid(neuronlayers[layer][j].value);
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
                    neuronlayers[i][j].value = Sigmoid(neuronlayers[i][j].value);
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
