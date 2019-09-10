using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace InventorySystem.Version2
{
    public static class FlowExtensions
    {
        public static int TotalDemand(this IEnumerable<IFlowSink> sinks)
        {
            return sinks.Sum(sink => sink.Demand);
        }
        public static int TotalSupply(this IEnumerable<IFlowSource> sources)
        {
            return sources.Sum(source => source.Supply);
        }

        public static int DrainSupplyFlowLimited(IFlowSource source, int demand, int flow, out int excessFlow)
        {
            var demandFlowed = math.min(demand, flow);
            var excessDemand = source.DrainSupply(demandFlowed);
            var supplied = demand - excessDemand;
            excessFlow = flow - supplied;
            return excessDemand;
        }
        public static int FillDemandFlowLimited(IFlowSink sink, int supply, int flow, out int excessFlow)
        {
            var supplyFlow = math.min(supply, flow);
            var excessSupply = sink.FillDemand(supplyFlow);
            var demanded = supply - excessSupply;
            excessFlow = flow - demanded;
            return excessSupply;
        }

        public static int DistributeSupplyFlow(this IEnumerable<IFlowSink> sinks, int supply, int flow,
            out int excessFlow)
        {
            throw new Exception();
            var temp = new Queue<IFlowSink>(sinks);
            var supplyFlow = math.min(supply, flow);
            var supplyUsed = 0;
            var average = supplyFlow / temp.Count;
            while (supplyUsed < supplyFlow && temp.Count > 0 && average > 0)
            {
                var c = temp.Count;
                for (var i = 0; i < c; i++)
                {
//                    sinks
                    
                }
            }
            


        }
    }
}