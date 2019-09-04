namespace InventorySystem.Version2
{
    public class PowerFlowNetworkIdea
    {
        void TickV3()
        {
            //Adjacent networks with the same flow are sibling networks
            //    Siblings have a slave/master relationship
            //    The flow direction determines who is the master, and who is the slave
            //        Slaves act as children for the purpose of this simulation

            //Adjacent networks with greater flow are parent networks
            //    Ergo less flow are child networks

            //For each network
            //    Determine...
            //        Determine Supplied
            //        Determine Requested
            //        Determine Flow Used
            //        Determine Flow Demand (Flow requested due to demand)
            //    Then...
            //        Determine Excess Supply (Not limited by Flow)
            //        Determine Excess Flow
            //        Determine Excess Demand (Not limited by Flow)


            //Next, we try to resolve child networks
            //    If Flow Negative, Nothing Requested, or Nothing Suplied
            //        Network will resolve immediately, and is ignored by parent
            //    If Flow Positive
            //        ...And Supplied, parent uses as a supplier
            //            If parent resolves as a sink, resolve all children immediately
            //        ...And demanded, parent uses as a sink
            //            If parent resolves as a source, resolve all children immediately
            //        Both cases are limited by Flow Remaining
            //        Both cases, children resolve if parent resolve as opposite


            //Once at the top, we immediately resolve all child networks, and ourselves

            //The simulation is now complete
        }


        void TickV2()
        {
            //Networks are divided by FLOW
            //Child networks have less flow

            //Gather all child networks
            //Resolve all child networks, ignoring parent networks, determine remaining flow

            //Gather power producers

            //Gather power buffers

            //Gather power sinks

            //Accumulate power in network

            //While flow allows it
            //Distribute power to sinks equally (treat child networks as sinks)
            //Gather excess power for redistribution

            //Redistribute excess
            //Repeat until sinks full or at capacity

            //Repeat child networks


            //Discharge power from producers

            //Discharge power from buffers

            //Report Flow To Parent
        }


        void Tick()
        {
            //Laymans terms; gather all power in the network, and distribute it evenly, regardless of flow or capacity


            //Gather power producers

            //Gather power buffers

            //Gather power sinks

            //Accumulate power in network

            //Distribute power to sinks equally
            //Gather excess

            //Redistribute excess
            //Repeat until sinks full

            //Discharge power from producers

            //Discharge power from buffers
        }
    }
}