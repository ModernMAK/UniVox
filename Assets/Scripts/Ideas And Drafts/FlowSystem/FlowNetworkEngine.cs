//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Types;
//using Unity.Mathematics;
//using UnityEntityManager = Unity.Entities.EntityManager;
//
//namespace Univox
//{
//    //An enum representing 3D Axis Ordering
//}
//
//namespace InventorySystem.Version2
//{
//    public class FlowNetworkEngine
//    {
//        public class FlowData : IFlowSink, IFlowSource
//        {
//            public FlowData(int supply, int demand, int flowCapacity)
//            {
//                Supply = supply;
//                Demand = demand;
//                FlowCapacity = flowCapacity;
//            }
//
//            public int Supply { get; set; }
//
//            public int DrainSupply(int demand)
//            {
//                var allowed = math.min(demand, ExcessFlow);
//                Demand += allowed;
//
//                return demand - allowed;
//            }
//
////        public FlowData AddSupply(int amount) => new FlowData(Supply + amount, Demand, FlowCapacity);
////        public FlowData SetSupply(int supply) => new FlowData(supply, Demand, FlowCapacity);
//            public int Demand { get; set; }
//
//            public int FillDemand(int supply)
//            {
//                var allowed = math.min(supply, ExcessFlow);
//                Supply += allowed;
//
//                return supply - allowed;
//            }
////        public FlowData AddDemand(int amount) => new FlowData(Supply, Demand + amount, FlowCapacity);
////        public FlowData SetDemand(int demand) => new FlowData(Supply, demand, FlowCapacity);
//
//
//            /// <summary>
//            /// The Supply used to fill demand
//            /// </summary>
//            private int SupplyUsed => math.min(Supply, Demand);
//
//            public int FlowCapacity { get; }
//
//            /// <summary>
//            /// The Flow used to fill demand, clamped to capacity
//            /// </summary>
//            public int FlowUsed => math.min(SupplyUsed, FlowCapacity);
//
//            /// <summary>
//            /// Excess supply,after filling demand
//            /// </summary>
//            public int ExcessSupply => Supply - SupplyUsed;
//
//            /// <summary>
//            /// Excess flow after filling demand (clamped to capacity)
//            /// </summary>
//            public int ExcessFlow => FlowCapacity - FlowUsed;
//
//            /// <summary>
//            /// Excess demand after filling from supply
//            /// </summary>
//            public int ExcessDemand => Demand - SupplyUsed;
//
//            /// <summary>
//            /// Excess supply, clamped by excess flow
//            /// </summary>
//
//            public int ExcessSupplyFlow => math.min(ExcessSupply, ExcessFlow);
//
//            /// <summary>
//            /// Excess demand, clamped by excess flow
//            /// </summary>
//            public int ExcessDemandFlow => math.min(ExcessDemand, ExcessFlow);
//
//            public bool CanFlow => ExcessFlow > 0f;
//            public bool CanSource => ExcessSupply > 0f;
//            public bool CanSink => ExcessDemand > 0f;
//        }
//
//        private class FlowNetworkEntry
//        {
//            public FlowNetworkEntry(IFlowNetwork ifn)
//            {
//            }
//
//            public int Supply => Sources.Sum(source => source.Supply);
//            public int Demand => Sinks.Sum(sink => sink.Demand);
//
//            public int FlowUsed { get; private set; }
//
//
//            public void Equalize()
//            {
//                var requestedFlow = math.min(Supply, Demand);
//                var clampedFlow = math.min(requestedFlow, FlowAvailable);
//            }
//
//
//            public int DrainSources(int demand)
//            {
//                var temp = new List<IFlowSource>(Sources);
//                var avg = 0;
//                do
//                {
//                    //Distribute by averaging
//                    avg = demand / temp.Count;
//                    //Drain demand
//                    demand -= avg * temp.Count;
//
//                    //Accumulate excess
//                    for (var i = 0; i < temp.Count; i++)
//                    {
//                        var source = temp[i];
//                        var excess = source.DrainSupply(avg); //REturns excess
//                        demand += excess;
//
//                        if (excess > 0)
//                        {
//                            temp.RemoveAt(i);
//                            i--;
//                        }
//                    }
//                } while (temp.Count > 0 && avg > 0 && demand > 0);
//
//                //Distribute remainder
//                foreach (var source in temp)
//                    demand = source.DrainSupply(demand);
//                return demand;
//            }
//
//            public int FillSinks(int supply)
//            {
//                throw new Exception();
////                var originalSupply;
//                var temp = new List<IFlowSink>(Sinks);
//                var avg = 0;
//                do
//                {
//                    //Distribute by averaging
//                    avg = supply / temp.Count;
//                    //Drain demand
//                    supply -= avg * temp.Count;
//
//                    //Accumulate excess
//                    for (var i = 0; i < temp.Count; i++)
//                    {
//                        var sink = temp[i];
//                        var excess = sink.FillDemand(avg); //REturns excess
//                        supply += excess;
//
//                        if (excess > 0)
//                        {
//                            temp.RemoveAt(i);
//                            i--;
//                        }
//                    }
//                } while (temp.Count > 0 && avg > 0 && supply > 0);
//
//                //Distribute remainder
//                foreach (var source in temp)
//                    supply = source.FillDemand(supply);
//                return supply;
//            }
//
//            public int FlowCapacity { get; }
//            public int FlowAvailable => FlowCapacity - FlowUsed;
//
//            public IReadOnlyList<IFlowSink> Sinks => throw new NotImplementedException();
//
//            public IReadOnlyList<IFlowSource> Sources => throw new NotImplementedException();
//        }
//
//
//        private static FlowData CreateFlowData(IFlowNetwork ifn)
//        {
//            return new FlowData(ifn.Supply, ifn.Demand, ifn.FlowCapacity);
//        }
//
//
//        private static IDictionary<IFlowNetwork, FlowNetworkPass> GatherFlowNetworkData(FlowNetwork root)
//        {
//            var flowNetworkData = new Dictionary<IFlowNetwork, FlowNetworkPass>();
//            GatherFlowNetworkData(root, flowNetworkData);
//            return flowNetworkData;
//        }
//
//        private static void GatherFlowNetworkData(IFlowNetwork root,
//            IDictionary<IFlowNetwork, FlowNetworkPass> networkData)
//        {
//            if (networkData.TryGetValue(root, out _))
//                throw new Exception("Cycle detected!");
//
//
//            var sources = new List<IFlowSource>(root.Sources);
//            var sinks = new List<IFlowSink>(root.Sinks);
//            var flow = CreateFlowData(root);
//            networkData[root] = new FlowNetworkPass(sources, sinks, flow);
//
//
//            foreach (var childFn in root.ChildNetworks)
//            {
//                GatherFlowNetworkData(childFn, networkData);
//            }
//        }
//
//        public class FlowNetworkPass
//        {
//            public FlowNetworkPass(ICollection<IFlowSource> sources, ICollection<IFlowSink> sinks, FlowData data)
//            {
//                Sources = sources;
//                Sinks = sinks;
//                Data = data;
//            }
//
//            public ICollection<IFlowSource> Sources { get; }
//            public ICollection<IFlowSink> Sinks { get; }
//            public FlowData Data { get; }
//        }
//
//
//        private static bool Resolve(IFlowNetwork root, IDictionary<IFlowNetwork, FlowNetworkPass> networkData)
//        {
//            var data = networkData[root];
//            foreach (var child in root.ChildNetworks)
//            {
//                Resolve(child, networkData);
//
//                ResolveChild(child, networkData, data);
//            }
//
//
////            root.DrainSupply(flowData.FlowUsed);
////            root.FillDemand(flowData.FlowUsed);
//
//
////            data.
//            throw new Exception();
//        }
//
//        private static void ResolveChild(IFlowNetwork root, IDictionary<IFlowNetwork, FlowNetworkPass> networkData,
//            FlowNetworkPass parent)
//        {
//            if (!networkData.TryGetValue(root, out var data))
//            {
//                throw new Exception("Network missing data!");
//            }
//
//            var flowData = data.Data;
//
//            if (!flowData.CanFlow) return;
//
//
//            if (flowData.CanSource)
//            {
//                parent.Sources.Add(root);
//            }
//            else if (flowData.CanSink)
//            {
//                parent.Sinks.Add(root);
//            }
//        }
//    }
//}

