using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using InventorySystem.Version2;
using Unity.Mathematics;
using UnityEditor.PackageManager;

namespace InventorySystem.Version2
{
    public enum FlowMode : byte
    {
        FIFO = 0,
        FirstComeFirstServe = 0,
        Equal
    }

    
    
    //OKAY Problems im seeing
    /* How to limit flow? Who knows this. Limiting flow when resolving children, need to keep track somehow.
     * Source, Buffer, Sink,; Sources want to give power, sinks want to take power, Buffers want to flip between the two
     * For buffers to work, networks need to know if they are Sources or Sinks, Multiple Passes?
     * Add Transmitter?
    */
    
    
    
    /* Im going to say this now... SCREW IT. Keep it simple, its a prototype.
     * Flow is the biggest problem to simlictty, simply remove it. We now have a simple model
     * Gather Sources, Sinks and Buffers
     * Determine if Network is Overall Source or Sink
     * Fianlize with Buffers treated as...
     *    Sources if network is a sink
     *    Sinks if the network is a source
     */
    

    public static class FlowExtensions
    {
        public static int GetTotalSupply(this IEnumerable<IFlowSource> sources) => sources.Sum(source => source.Supply);
        public static int GetTotalDemand(this IEnumerable<IFlowSink> sinks) => sinks.Sum(sink => sink.Demand);

        private static Func<IFlowSource, int, int> DrainSupplySurrogate =>
            (source, demand) => source.DrainSupply(demand);

        private static Func<IFlowSink, int, int> FillDemandSurrogate =>
            (sink, supply) => sink.FillDemand(supply);

        public static int DrainSupply(this IEnumerable<IFlowSource> sources, int supply, FlowMode mode = FlowMode.FirstComeFirstServe)
        {
            var surrogate = DrainSupplySurrogate;
            return InternalFlowLogic(sources, supply, surrogate, mode);
        }


        public static int FillDemand(this IEnumerable<IFlowSink> sinks, int demand,
            FlowMode mode = FlowMode.FirstComeFirstServe)
        {
            var surrogate = FillDemandSurrogate;
            return InternalFlowLogic(sinks, demand, surrogate, mode);
        }

        private static int InternalFlowLogic<T>(this IEnumerable<T> data, int value, Func<T, int, int> surrogate,
            FlowMode mode)
        {
            switch (mode)
            {
                case FlowMode.FIFO:
                    return InternalFlowFIFO(data, value, surrogate);
                case FlowMode.Equal:
                    return InternalFlowEqualize(data, value, surrogate);
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private static int InternalFlowFIFO<T>(this IEnumerable<T> data, int value, Func<T, int, int> func)
        {
            foreach (var d in data)
            {
                if (value > 0f)
                {
                    value = func(d, value);
                }
                else break;
            }

            return value;
        }

        private static int InternalFlowEqualize<T>(this IEnumerable<T> data, int value, Func<T, int, int> func)
        {
            throw new NotImplementedException();
//            if (count <= 0)
//                return value;
//
//            var avg = value / count;
//            var remainder = value % count;
//
//            if (avg <= 0)
//            {
//                foreach (var d in data)
//                {
//                    if (remainder > 0)
//                    {
//                        remainder += func(d, 1);
//                        remainder--;
//                    }
//                    else break;
//                }
//            }
//            else
//            {
//                foreach (var d in data)
//                {
//                    if (remainder > 0)
//                    {
//                        remainder += func(d, 1);
//                        remainder--;
//                    }
//                    else break;
//                }
//            }
        }
    }


    public class FlowNetworkEngine
    {
        public class FlowData : IFlowSink, IFlowSource
        {
            public FlowData(int supply, int demand, int flowCapacity)
            {
                Supply = supply;
                Demand = demand;
                FlowCapacity = flowCapacity;
            }

            public int Supply { get; set; }

            public int DrainSupply(int demand)
            {
                var allowed = math.min(demand, ExcessFlow);
                Demand += allowed;

                return demand - allowed;
            }

//        public FlowData AddSupply(int amount) => new FlowData(Supply + amount, Demand, FlowCapacity);
//        public FlowData SetSupply(int supply) => new FlowData(supply, Demand, FlowCapacity);
            public int Demand { get; set; }

            public int FillDemand(int supply)
            {
                var allowed = math.min(supply, ExcessFlow);
                Supply += allowed;

                return supply - allowed;
            }
//        public FlowData AddDemand(int amount) => new FlowData(Supply, Demand + amount, FlowCapacity);
//        public FlowData SetDemand(int demand) => new FlowData(Supply, demand, FlowCapacity);


            /// <summary>
            /// The Supply used to fill demand
            /// </summary>
            private int SupplyUsed => math.min(Supply, Demand);

            public int FlowCapacity { get; }

            /// <summary>
            /// The Flow used to fill demand, clamped to capacity
            /// </summary>
            public int FlowUsed => math.min(SupplyUsed, FlowCapacity);

            /// <summary>
            /// Excess supply,after filling demand
            /// </summary>
            public int ExcessSupply => Supply - SupplyUsed;

            /// <summary>
            /// Excess flow after filling demand (clamped to capacity)
            /// </summary>
            public int ExcessFlow => FlowCapacity - FlowUsed;

            /// <summary>
            /// Excess demand after filling from supply
            /// </summary>
            public int ExcessDemand => Demand - SupplyUsed;

            /// <summary>
            /// Excess supply, clamped by excess flow
            /// </summary>

            public int ExcessSupplyFlow => math.min(ExcessSupply, ExcessFlow);

            /// <summary>
            /// Excess demand, clamped by excess flow
            /// </summary>
            public int ExcessDemandFlow => math.min(ExcessDemand, ExcessFlow);

            public bool CanFlow => ExcessFlow > 0f;
            public bool CanSource => ExcessSupply > 0f;
            public bool CanSink => ExcessDemand > 0f;
        }

        private class FlowNetworkEntry : IFlowNetwork
        {
            public FlowNetworkEntry(IFlowNetwork flowNetwork)
            {
                _flowNetwork = flowNetwork;
            }

            private readonly IFlowNetwork _flowNetwork;

            public int Supply => math.min(Sources.GetTotalSupply(), FlowAvailable);

            public int DrainSupply(int demand)
            {
                var demandFlow = math.min(FlowAvailable, demand);
                var unfilledDemandFlow = _flowNetwork.DrainSupply(demandFlow);
                var supplyUsed = demandFlow - unfilledDemandFlow;
                FlowUsed += supplyUsed;
                
                //We need to return the excess demand
                return demand - supplyUsed;
            }

            public int Demand => math.min(Sinks.GetTotalDemand(), FlowAvailable);

            public int FillDemand(int supply)
            {
                var supplyFlow = math.min(FlowAvailable, supply);
                var unfilledSupplyFlow = _flowNetwork.FillDemand(supplyFlow);
                var demandUsed = supplyFlow - unfilledSupplyFlow;
                FlowUsed += demandUsed;
                
                //We need to return the excess supply
                return supply - demandUsed;
            }

            public int FlowUsed { get; private set; }


            public void Initialize()
            {
                var flowDemand = Demand;
                var flowSupply = Supply;
                var min = math.min(flowDemand, flowSupply);

                _flowNetwork.FillDemand(min);
                _flowNetwork.DrainSupply(min);
                FlowUsed += min;
            }


            public int FlowCapacity { get; }

            public FlowMode FlowMode => _flowNetwork.FlowMode;

            public int FlowAvailable => FlowCapacity - FlowUsed;

            public IReadOnlyList<IFlowSink> Sinks => _flowNetwork.Sinks;

            public IReadOnlyList<IFlowSource> Sources => _flowNetwork.Sources;

            public IReadOnlyList<IFlowNetwork> ChildNetworks => _flowNetwork.ChildNetworks;
        }


        private static FlowData CreateFlowData(IFlowNetwork ifn)
        {
            return new FlowData(ifn.Supply, ifn.Demand, ifn.FlowCapacity);
        }


        private static IDictionary<IFlowNetwork, FlowNetworkPass> GatherFlowNetworkData(FlowNetwork root)
        {
            var flowNetworkData = new Dictionary<IFlowNetwork, FlowNetworkPass>();
            GatherFlowNetworkData(root, flowNetworkData);
            return flowNetworkData;
        }

        private static void GatherFlowNetworkData(IFlowNetwork root,
            IDictionary<IFlowNetwork, FlowNetworkPass> networkData)
        {
            if (networkData.TryGetValue(root, out _))
                throw new Exception("Cycle detected!");


            var sources = new List<IFlowSource>(root.Sources);
            var sinks = new List<IFlowSink>(root.Sinks);
            var flow = CreateFlowData(root);
            networkData[root] = new FlowNetworkPass(sources, sinks, flow);


            foreach (var childFn in root.ChildNetworks)
            {
                GatherFlowNetworkData(childFn, networkData);
            }
        }

        public class FlowNetworkPass
        {
            public FlowNetworkPass(ICollection<IFlowSource> sources, ICollection<IFlowSink> sinks, FlowData data)
            {
                Sources = sources;
                Sinks = sinks;
                Data = data;
            }

            public ICollection<IFlowSource> Sources { get; }
            public ICollection<IFlowSink> Sinks { get; }
            public FlowData Data { get; }
        }


        private static bool Resolve(IFlowNetwork root, IDictionary<IFlowNetwork, FlowNetworkPass> networkData)
        {
            var data = networkData[root];
            foreach (var child in root.ChildNetworks)
            {
                Resolve(child, networkData);

                ResolveChild(child, networkData, data);
            }


//            root.DrainSupply(flowData.FlowUsed);
//            root.FillDemand(flowData.FlowUsed);


//            data.
        }

        private static void ResolveChild(IFlowNetwork root, IDictionary<IFlowNetwork, FlowNetworkPass> networkData,
            FlowNetworkPass parent)
        {
            if (!networkData.TryGetValue(root, out var data))
            {
                throw new Exception("Network missing data!");
            }

            var flowData = data.Data;

            if (!flowData.CanFlow) return;


            if (flowData.CanSource)
            {
                parent.Sources.Add(root);
            }
            else if (flowData.CanSink)
            {
                parent.Sinks.Add(root);
            }
        }
    }
}