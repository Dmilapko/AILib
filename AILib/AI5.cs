using MonoHelper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AILib
{
    [Serializable()]
    public class AI5:AIFamilyEvo0 
    {
        [Serializable()]
        public class Connection : ConnectionFamilyEvo0 
        {
            public double weight = 0;
            public double max_flow = 0;

            public void RandWeight()
            {
                weight += MHeleper.RandomDouble() * 2 - 1;
                if (weight > 1) weight = 1;
                if (weight < -1) weight = -1;
            }

            public Connection(int to_neuron)
            {
                this.to_neuron = to_neuron;
                RandWeight();
            }
        }
        [Serializable()]
        public class Neuron:NeuronFamilyEvo0
        {
            public double bias = 0;

            public void RandBias()
            {
                bias = MHeleper.RandomDouble() * 2 - 1;
                if (bias > 1) bias = 1;
                if (bias < -1) bias = -1;
            }

            public Neuron()
            {
                value = 0;
                ///----
            }

            public override void Pre_Calculation()
            {
                value += bias;
            }

            public override void After_Calculation()
            {
                //value = 1.0 / (1.0 + Math.Exp(-value)) - 0.5;
                value = (1.0 / (1.0 + Math.Exp(-value)) - 0.5) * 2;
            }

            public override double Calculate_Connection(int connection_ind)
            {
                double res = value * ((Connection)connections[connection_ind]).weight;
                ((Connection)connections[connection_ind]).max_flow = Math.Max(res, Math.Abs(((Connection)connections[connection_ind]).max_flow));
                return res;
            }
        }

        public double mutations_per_generation = 1;
        double maxopc;
        public int generation = 0;

     /*   public override void SetInput(int input_number, double _value)
        {
            _value += ((Neuron)neurons[input_number]).bias;
            base.SetInput(input_number, _value);
        }*/

        public AI5(int iput_count, int output_count, int maxopc):base(iput_count, output_count)
        {
            this.maxopc = maxopc;
            mutations_per_generation = 1 + MHeleper.RandomDouble() * (maxopc - 1);
        }

        const int ADD_CONNECTION = 0, DELETE_CONNECTION = 1, CHANGE_CONNECTION = 2, ADD_NEURON = 3, CHANGE_NEURON = 4;

        public override void MutateAI()
        {
            generation++;
            for (int i = 0; i < neurons.Count; i++)
            {
                for (int j = 0; j < neurons[i].connections.Count; j++)
                {
                    if (((AI5.Connection)neurons[i].connections[j]).max_flow < 0.01)
                    {
                    //    DeleteConnection(i, neurons[i].connections[j].to_neuron);
                      //  j--;
                    }
                    else ((AI5.Connection)neurons[i].connections[j]).max_flow = 0;
                    ((AI5.Connection)neurons[i].connections[j]).max_flow = 0;
                }
            }
            if (MHeleper.RandomElement(new List<double>(){ 1, all_connections.Count / mutations_per_generation}) == 0)
            {
                mutations_per_generation = 1 + MHeleper.RandomDouble() * (maxopc - 1);
            }
            /*if (MHeleper.RandomElement(new List<double>() { 1, maxopc }) == 0)
            {
                mutations_per_generation = 1 + MHeleper.RandomDouble() * (maxopc - 1);
            }*/
            int opc = MHeleper.RandomRound(mutations_per_generation);
            if (opc <= 0) opc = 1;
            for (int i = 0; i < opc * 2; i++)
            {
                if (i!=0 && MHeleper.RandomDouble() > 0.5) continue;
                SyncData();
                List<double> mutation_probabilities = new List<double>() { all_possible_connections.Count, all_connections.Count, all_connections.Count, all_connections.Count, alive_neurons.Count };
                /*mutation_probabilities.Add((neurons.Count - killed_neurons_count) * (Math.Max(all_connections.Count, 1) / (double)Math.Max(alive_neurons_count, 1)));
                mutation_probabilities.Add(all_connections.Count);
                mutation_probabilities.Add(all_connections.Count);
                mutation_probabilities.Add(alive_neurons_count);
                mutation_probabilities.Add(alive_neurons_count);*/
                if (all_connections.Count != 0)
                {
                }
                int operation_type = MHeleper.RandomElement(mutation_probabilities);
                switch (operation_type)
                {
                    case ADD_CONNECTION:
                        AddRandomConnection(all_possible_connections);
                        break;
                    case DELETE_CONNECTION:
                        DeleteRandomConnection(all_connections);
                        break;
                    case CHANGE_CONNECTION:
                        ChangeRandomConnection(all_connections);
                        break;
                    case ADD_NEURON:
                        AddRandomNeuron(all_connections);
                        break;
                    case CHANGE_NEURON:
                        ChangeRandomNeuron();
                        break;
                    default:
                        throw new Exception("no such operation");
                }
            }
            foreach (AI5.Neuron item in neurons)
            {
                if (item.connections.Count == 0) item.bias = 0;
            }
            base.MutateAI();
        }

        public void ChangeRandomConnection(List<Pair<int, int>> connections)
        {
            int n = MHeleper.RandomInt(0, connections.Count);
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[n].Second == neurons[connections[n].First].connections[i].to_neuron) ((Connection)neurons[connections[n].First].connections[i]).RandWeight();
                break;
            }
        }

        public void ChangeRandomNeuron()
        {
            int n = MHeleper.RandomInt(0, alive_neurons.Count);
            ((Neuron)neurons[alive_neurons[n]]).RandBias();
        }

        public override void CreateNewConnection(int from, int to)
        {
            neurons[from].connections.Add(new Connection(to));
        }

        public override void CreateNewNeuron()
        {
            neurons.Add(new Neuron());
        }

        public AI5 GetCopy()
        {
            AI5 res = MHeleper.CreateDeepCopy(this);
            foreach (var item in res.neurons)
            {
                item.value = 0;
            }
            return res;
        }

        public void Reset()
        {
            foreach (Neuron neuron in neurons)
            {
                neuron.value = 0;
            }
        }
    }
}
