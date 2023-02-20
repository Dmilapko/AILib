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

            public void RandWeight(double goodness)
            {
                weight += MHeleper.RandomDouble() * 2 - 1;
                if (weight > 1) weight = 1;
                if (weight < -1) weight = -1;
            }

            public Connection(int to_neuron)
            {
                this.to_neuron = to_neuron;
                RandWeight(0);
            }
        }
        [Serializable()]
        public class Neuron:NeuronFamilyEvo0
        {
            double bias = 0;

            public void RandBias(double goodness)
            {
                bias = MHeleper.RandomDouble() * 2 - 1;
                if (bias > 1) bias = 1;
                if (bias < -1) bias = -1;
            }

            public Neuron()
            {
               
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
                return value * ((Connection)connections[connection_ind]).weight;
            }
        }

        public double mutations_per_generation = 1;
        double maxopc;

        public AI5(int iput_count, int output_count, int maxopc):base(iput_count, output_count)
        {
            this.maxopc = maxopc;
        }

        const int ADD_CONNECTION = 0, DELETE_CONNECTION = 1, CHANGE_CONNECTION = 2, ADD_NEURON = 3, CHANGE_NEURON = 4;

        public override void MutateAI()
        {
            generation++;
            if (MHeleper.RandomElement(new List<double>(){ 1, all_connections.Count / mutations_per_generation}) == 0)
            {
                mutations_per_generation = 1 + MHeleper.RandomDouble() * (maxopc - 1);
            }
            int opc = MHeleper.RandomRound(mutations_per_generation);
            if (opc == 0) opc = 1;
            for (int i = 0; i < opc; i++)
            {
                SyncData();
                List<double> mutation_probabilities = new List<double>() { Math.Min(all_possible_connections.Count, 1), Math.Min(all_connections.Count, 1), Math.Min(all_possible_connections.Count, 1), 0};
                if (all_connections.Count != 0) mutation_probabilities.Add(Math.Min(1, (double)neurons.Count / all_connections.Count));
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
            base.MutateAI();
        }

        public void ChangeRandomConnection(List<Pair<int, int>> connections)
        {
            int n = MHeleper.RandomInt(0, connections.Count);
            for (int i = 0; i < connections.Count; i++)
            {
                if (connections[n].Second == neurons[connections[n].First].connections[i].to_neuron) ((Connection)neurons[connections[n].First].connections[i]).RandWeight(goodness);
                break;
            }
        }

        public void ChangeRandomNeuron()
        {
            int n = MHeleper.RandomInt(0, neurons.Count);
            ((Neuron)neurons[n]).RandBias(goodness);
        }

        public override void CreateNewConnection(int from, int to)
        {
            neurons[from].connections.Add(new Connection(to));
        }

        public override void CreateNewNeuron()
        {
            neurons.Add(new Neuron());
        }
    }
}
