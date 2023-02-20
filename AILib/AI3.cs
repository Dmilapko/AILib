using System;
using System.Collections.Generic;
using System.Runtime;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using MonoHelper;

namespace AILib
{
    /// <summary>
    /// TEST VESRSION. First of the "if" ais.
    /// GLORY TO UKRAINE.
    /// GLORY TO GOLꑭOV
    /// </summary>
    public class AI3:AIFamily0
    {
        public class AI3Connection
        {
            public double s = 0.5;
            public double w = 0;
            private double wd = 30;

            public AI3Connection() { }

            public AI3Connection(double diap)
            {
                RandWeight(diap);
                RandBias(diap);
            }

            public AI3Connection(AI3Connection parent, double diap)
            {
                s = parent.s;
                w = parent.w;
                RandWeight(diap);
                RandBias(diap);
            }

            private void RandWeight(double diap)
            {
                diap *= wd;
                double offset = 0;
                diap *= 2;
                if ((w + diap / 2) > wd)
                    offset = wd - diap / 2 - w;
                if ((w - diap / 2) < -1 * wd)
                    offset = -1 * w + diap / 2 - wd;
                w += (MHeleper.RandomDouble() - 0.5) * diap + offset;
                if (w > wd) w = wd;
                if (w < -1 * wd) w = -1 * wd;
            }

            private void RandBias(double diap)
            {
                double offset = 0;
                if ((s + diap / 2) > 1)
                    offset = 1 - diap / 2 - s;
                if ((s - diap / 2) < 0)
                    offset = -1 * s + diap / 2;
                s += (MHeleper.RandomDouble() - 0.5) * diap + offset;
                if (s > 1) s = 1;
                if (s < 0) s = 0;
            }
        }

        public class AI3Neuron
        {
            public double value = 0;
            public List<AI3Connection> connections = new List<AI3Connection>();

            public AI3Neuron() { }

            public AI3Neuron(int prevcnt, double diapason = 1)
            {
                for (int i = 0; i < prevcnt; i++)
                {
                    connections.Add(new AI3Connection(diapason));
                }
            }

            public AI3Neuron(AI3Neuron parent, double diapason)
            {
                for (int i = 0; i < parent.connections.Count; i++)
                {
                    connections.Add(new AI3Connection(parent.connections[i], diapason));
                }
            }
        }

        public List<List<AI3Neuron>> neuronlayers = new List<List<AI3Neuron>>();
        public List<AI3Neuron> inputs;
        public List<AI3Neuron> outputs;
        public int complexity = 0;


        public AI3(List<int> Size_of_Layers)
        { 
            for (int i = 0; i < Size_of_Layers.Count; i++)
            {
                neuronlayers.Add(new List<AI3Neuron>());
                for (int j = 0; j < Size_of_Layers[i]; j++) 
                {
                    int conc = 0;
                    if (i != 0) conc = neuronlayers[i - 1].Count;
                    neuronlayers[i].Add(new AI3Neuron(conc));
                    complexity += conc;
                }
            }
            Fin_Init();
        }

        public AI3(AI3 parent, double goodness)
        {
            //goodness = Math.Pow(Math.Sqrt(goodness), 1 / (double)(parent.neuronlayers.Count - 1));
            goodness = Math.Pow(goodness, 1 / (double)parent.neuronlayers.Count);
            generation = parent.generation;
            mygoodness = parent.mygoodness;
            for (int i = 0; i < parent.neuronlayers.Count; i++)
            {
                neuronlayers.Add(new List<AI3Neuron>());
                for (int j = 0; j < parent.neuronlayers[i].Count; j++)
                {
                    neuronlayers[i].Add(new AI3Neuron(parent.neuronlayers[i][j], 1-goodness));
                }
            }
            Fin_Init();
        }

        private void Fin_Init()
        {
            inputs = new List<AI3Neuron>(neuronlayers[0]);
            outputs = new List<AI3Neuron>(neuronlayers[neuronlayers.Count - 1]);
        }

        public static double Sigmoid(double value)
        {
            return 1.0 / (1.0 + Math.Exp(-value));
        }

        public AI3()
        {

        }

        public void SaveToFile(string filename)
        {
            File.WriteAllText(filename, JsonConvert.SerializeObject(this));
        }

        public static AI3 ReadFromFile(string filename)
        {
            return JsonConvert.DeserializeObject<AI3>(File.ReadAllText(filename));
        }


        public void Run()
        {
            for (int i = 1; i < neuronlayers.Count; i++)
            {
                for (int j = 0; j < neuronlayers[i].Count; j++)
                {
                    neuronlayers[i][j].value = 0;
                    for (int c = 0; c < neuronlayers[i][j].connections.Count; c++)
                    {
                        neuronlayers[i][j].value += (neuronlayers[i - 1][c].value - neuronlayers[i][j].connections[c].s) * neuronlayers[i][j].connections[c].w;
                    }
                    neuronlayers[i][j].value = Sigmoid(neuronlayers[i][j].value / (double)neuronlayers[i][j].connections.Count);
                }
            }
        }
    }
}
