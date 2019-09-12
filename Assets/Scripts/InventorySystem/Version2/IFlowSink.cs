using System.Collections.Generic;

namespace InventorySystem.Version2
{
    
    /// <summary>
    /// Represents a 'Sink' which knows it's current demand, and allows demand to be 'Filled'
    /// </summary>
    public interface IFlowSink
    {
        /// <summary>
        /// The current demand of the sink. Expected to be non-negative.
        /// </summary>
        int Demand { get; }
        
        /// <summary>
        /// Decreases the sink's demand. Will return excess supply.
        /// </summary>
        /// <param name="supply">The amount of demand to decrease.</param>
        /// <returns>The excess supply.</returns>
        int FillDemand(int supply);
    }
}

namespace FlowSystem
{
    public interface IFlowSource
    {
        int Supply { get; }
        int EmptySource(int amount);
    }

    public interface IFlowSink
    {
        int Demand { get; }
        int FillSink(int amount);
    }

    public interface IFlowBuffer
    {
        int Stored { get; }
        int Capacity { get; }
        int FillBuffer(int amount);
        int EmptyBuffer(int amount);
    }

    namespace UnlimitedFlow
    {

        public class FlowNetwork : IFlowNetworkHandler, IFlowNetwork
        {
            public bool AddSource(IFlowSource source)
            {
                return InternalSources.AddUnique(source);
            }

            public bool RemoveSource(IFlowSource source)
            {
                throw new System.NotImplementedException();
            }

            public bool AddSink(IFlowSink sink)
            {
                return InternalSinks.AddUnique(sink);
            }

            public bool RemoveSink(IFlowSink sink)
            {
                return InternalSinks.Remove(sink);
            }

            public bool AddBuffer(IFlowBuffer buffer)
            {
                return InternalBuffers.AddUnique(buffer);
            }

            public bool RemoveBuffer(IFlowBuffer buffer)
            {
                return InternalBuffers.Remove(buffer);
            }

            private ICollection<IFlowSource> InternalSources { get; }
            private ICollection<IFlowSink> InternalSinks { get; }
            private ICollection<IFlowBuffer> InternalBuffers { get; }
            public IEnumerable<IFlowSource> Sources => InternalSources;
            public IEnumerable<IFlowSink> Sinks => InternalSinks;
            public IEnumerable<IFlowBuffer> Buffers => InternalBuffers;
        }

        interface IFlowNetwork
        {
            IEnumerable<IFlowSource> Sources { get; }
            IEnumerable<IFlowSink> Sinks { get; }
            IEnumerable<IFlowBuffer> Buffers { get; }
        }
    }
}