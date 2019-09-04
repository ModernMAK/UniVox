using System.Collections.Generic;

namespace InventorySystem.Version2
{
    public interface IFlowNetwork : IFlowSource, IFlowSink
    {
        int FlowCapacity { get; }
        IReadOnlyList<IFlowSink> Sinks { get; }
        IReadOnlyList<IFlowSource> Sources { get; }
        IReadOnlyList<IFlowNetwork> ChildNetworks { get; }
    }
}