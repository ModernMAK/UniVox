using System.Collections.Generic;
using System.Linq;

namespace InventorySystem.Version2
{
    public class FlowNetwork : IFlowNetwork
    {
        public int FlowCapacity { get; }

        public IReadOnlyList<IFlowSink> Sinks { get; }
        public IReadOnlyList<IFlowNetwork> ChildNetworks { get; }
        public IReadOnlyList<IFlowSource> Sources { get; }


        public int GatherSourceSupply()
        {
            return Sources.Sum(source => source.Supply);
        }

        public int GatherSinkDemand()
        {
            return Sinks.Sum(sink => sink.Demand);
        }


        public int Supply => GatherSourceSupply();


        public int DrainSupply(int demand)
        {
            var temp = new List<IFlowSource>(Sources);
            var avg = 0;
            do
            {
                //Distribute by averaging
                avg = demand / temp.Count;
                //Drain demand
                demand -= avg * temp.Count;

                //Accumulate excess
                for (var i = 0; i < temp.Count; i++)
                {
                    var source = temp[i];
                    var excess = source.DrainSupply(avg); //REturns excess
                    demand += excess;

                    if (excess > 0)
                    {
                        temp.RemoveAt(i);
                        i--;
                    }
                }
            } while (temp.Count > 0 && avg > 0 && demand > 0);

            //Distribute remainder
            foreach (var source in temp)
                demand = source.DrainSupply(demand);
            return demand;
        }

        public int Demand => GatherSinkDemand();


        public int FillDemand(int supply)
        {
            var temp = new List<IFlowSink>(Sinks);
            var avg = 0;
            do
            {
                //Distribute by averaging
                avg = supply / temp.Count;
                //Drain demand
                supply -= avg * temp.Count;

                //Accumulate excess
                for (var i = 0; i < temp.Count; i++)
                {
                    var sink = temp[i];
                    var excess = sink.FillDemand(avg); //REturns excess
                    supply += excess;

                    if (excess > 0)
                    {
                        temp.RemoveAt(i);
                        i--;
                    }
                }
            } while (temp.Count > 0 && avg > 0 && supply > 0);

            //Distribute remainder
            foreach (var source in temp)
                supply = source.FillDemand(supply);
            return supply;
        }
    }
}