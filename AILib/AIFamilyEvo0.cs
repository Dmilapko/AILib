using System;
using System.Collections.Generic;
using System.Runtime;
using Newtonsoft.Json;
using System.Threading;
using System.IO;
using MonoHelper;
using static AILib.AI5;

namespace AILib
{
    //In the name of Heinz Guderian, Golꑭov, UKRAINE
    /// Implements (mainly) graph structure of NEAT
    /// Have history
    /// </summary>
    [Serializable()]
    public abstract class AIFamilyEvo0
    {
        [Serializable()]
        public abstract class ConnectionFamilyEvo0 
        {
            public int to_neuron;
        }
        [Serializable()]
        public class NeuronFamilyEvo0
        {
            public bool alive = true;
            public double value = 0.5;
            internal int inputing_me = 0;
            public List<ConnectionFamilyEvo0> connections = new List<ConnectionFamilyEvo0>();

            public virtual void Pre_Calculation() { }

            public virtual void After_Calculation() { }

            public virtual double Calculate_Connection(int connection_id) { return 0; }
        }
        [Serializable()]
        public class HistoryEvent { }
        [Serializable()]
        public class AddedConnection : HistoryEvent 
        {
            public int from, to;
            public AddedConnection(int from_neuron, int to_neuron)
            {
                from = from_neuron;
                to = to_neuron;
            }
        }
        [Serializable()]
        public class AddedNeuron : HistoryEvent 
        {
            public int from, to, neuron_id;
            public AddedNeuron(int from_neuron, int to_neuron, int neuron_added_id)
            {
                from = from_neuron;
                to = to_neuron;
                neuron_id = neuron_added_id;
            }
        }
        [Serializable()]
        public class RemovedConnection : HistoryEvent 
        {
            public int from, to;
            public RemovedConnection(int from_neuron, int to_neuron)
            {
                from = from_neuron;
                to = to_neuron;
            }
        }

        public List<NeuronFamilyEvo0> neurons = new List<NeuronFamilyEvo0>();
        public int input_count, output_count;
        public bool alive = true;
        public List<HistoryEvent> history = new List<HistoryEvent>();
        public double goodness = 0;
        public int generation = 0;
        public int total_connections;
        public List<Pair<int, int>> all_connections = new List<Pair<int, int>>();
        public List<Pair<int, int>> all_possible_connections = new List<Pair<int, int>>();

        public abstract void CreateNewNeuron();

        public abstract void CreateNewConnection(int from, int to);

        public AIFamilyEvo0(int input_count, int output_count) 
        {
            total_connections = 0;
            this.input_count = input_count;
            this.output_count = output_count;
            for (int i = 0; i < input_count + output_count; i++) CreateNewNeuron();
            SyncData();
        }

        /// <summary>
        /// Set input. Value must be centred from -1 to 1
        /// </summary>
        /// <param name="input_number"></param>
        /// <param name="value"></param>
        public void SetInput(int input_number, double value)
        {
            neurons[input_number].value = value;
            neurons[input_number].value = Math.Min(1, neurons[input_number].value);
            neurons[input_number].value = Math.Max(-1, neurons[input_number].value);
        }

        public double GetOutput(int output_number)
        {
            return neurons[input_count + output_number].value;
        }

        public void SyncData()
        {
            all_possible_connections = PossibleConnections();
            all_connections = AllConnections();
        }

        public void AddConnection(int from, int to)
        {
            CreateNewConnection(from, to);
            history.Add(new AddedConnection(from, to));
            neurons[to].inputing_me++;
            total_connections++;
        }

        public void AddConnection(Pair<int, int> from_to) => AddConnection(from_to.First, from_to.Second);

        public void AddNeuron(int from, int to)
        {
            DeleteConnection(from, to, false);
            CreateNewNeuron();
            history.Add(new AddedNeuron(from, to, neurons.Count - 1));
            AddConnection(from, neurons.Count - 1);
            AddConnection(neurons.Count-1, to);
        }

        public virtual void MutateAI()
        {
            SyncData();
        }

        public void AddNeuron(Pair<int, int> from_to) => AddNeuron(from_to.First, from_to.Second);

        public void DeleteConnection(int from, int to, bool clear = true)
        {
            /*  for (int i = 0; i < neurons[from].connections.Count; i++)
              {
                  if (neurons[from].connections[i].to_neuron == to)
                  {
                      neurons[from].connections.RemoveAt(i);
                      break;
                  }
              }
              neurons[to].inputing_me--;
              for (int i = 0; i < neurons.Count; i++)
              {
                  if (neurons[i].inputing_me == 0)
                  {
                      for (int j = 0; j < neurons[i].connections.Count; j++)
                      {
                          neurons[neurons[i].connections[j].to_neuron].inputing_me--;
                      }
                      neurons.RemoveAt(i);
                      i = -1;
                  }
              }*/
            total_connections--;
            for (int i = 0; i < neurons[from].connections.Count; i++)
            {
                if (neurons[from].connections[i].to_neuron == to)
                {
                    neurons[from].connections.RemoveAt(i);
                    break;
                }
            }
            neurons[to].inputing_me--;
            if (clear)
            {
                for (int i = input_count + output_count; i < neurons.Count; i++)
                {
                    if ((neurons[i].inputing_me == 0) && neurons[i].alive)
                    {
                        for (int j = 0; j < neurons[i].connections.Count; j++)
                        {
                            neurons[neurons[i].connections[j].to_neuron].inputing_me--;
                            total_connections--;
                        }
                        neurons[i].alive = false;
                        i = input_count + output_count;
                    }
                }
            }
            history.Add(new RemovedConnection(from, to));
        }

        public void DeleteConnection(Pair<int, int> from_to) => DeleteConnection(from_to.First, from_to.Second);

        private List<Pair<int,int>> PossibleConnections()
        {
            List<Pair<int, int>> pairs = new List<Pair<int, int>>();
            for (int i = 0; i < neurons.Count; i++) if (neurons[i].alive && (i < input_count || neurons[i].inputing_me != 0)) 
            {
                for (int j = input_count; j < neurons.Count; j++) if (neurons[j].alive)
                {
                    if (i != j)
                    {
                        bool ok = true;
                        foreach (var connection in neurons[i].connections)
                        {
                            if (connection.to_neuron == j)
                            {
                                ok = false;
                                break;
                            }
                        }
                        if (ok)
                        {
                            List<bool> visited = new List<bool>();
                            for (int c = 0; c < neurons.Count; c++)
                            {
                                visited.Add(false);
                            }
                            List<int> need_go = new List<int> { j };
                            for (int c = 0; c < need_go.Count; c++)
                            {
                                foreach (var connection in neurons[need_go[c]].connections) if (!visited[connection.to_neuron])
                                {
                                    need_go.Add(connection.to_neuron);
                                    visited[connection.to_neuron] = true;
                                }
                                if (visited[i])
                                {
                                    ok = false;
                                    break;
                                }
                            }
                            if (ok) pairs.Add(new Pair<int, int>(i, j));
                        }
                    }
                }
            }
            return pairs;
        }

        public void DeleteRandomConnection(List<Pair<int, int>> possible_cons)
        {
            int n = MHeleper.RandomInt(0, possible_cons.Count);
            DeleteConnection(possible_cons[n]);
        }

        public void AddRandomConnection(List<Pair<int, int>> possible_cons)
        {
            if (possible_cons.Count == 0) return;
            int n = MHeleper.RandomInt(0, possible_cons.Count);
            AddConnection(possible_cons[n]);
        }

        private List<Pair<int, int>> AllConnections()
        {
            List<Pair<int, int>> pairs = new List<Pair<int, int>>();
            for (int i = 0; i < neurons.Count; i++) 
            {
                for (int j = 0; j < neurons[i].connections.Count; j++)
                {
                    pairs.Add(new Pair<int, int>(i, neurons[i].connections[j].to_neuron));
                }
            }
            return pairs;
        }

        public void AddRandomNeuron(List<Pair<int, int>> possible_neurons)
        {
            int n = MHeleper.RandomInt(0, possible_neurons.Count);
            AddNeuron(possible_neurons[n]);
        }


        public void Run()
        {
            List<int> need_get = new List<int>();
            for (int i = 0; i < neurons.Count; i++)
            {
                need_get.Add(neurons[i].inputing_me);
            }
            List<int> need_go = new List<int>(neurons.Count);
            for (int i = 0; i < input_count; i++)
            {
                need_go.Add(i);
            }
            for (int i = 0; i < need_go.Count; i++)
            {
                neurons[need_go[i]].Pre_Calculation();
                for (int j = 0; j < neurons[need_go[i]].connections.Count; j++)
                {
                    int to_neuron = neurons[need_go[i]].connections[j].to_neuron;
                    neurons[to_neuron].value += neurons[need_go[i]].Calculate_Connection(j);
                    need_get[to_neuron]--;
                    if (need_get[to_neuron] == 0)
                    {
                        neurons[to_neuron].After_Calculation();
                        need_go.Add(to_neuron);
                    }
                }
            }
        }
    }
}
