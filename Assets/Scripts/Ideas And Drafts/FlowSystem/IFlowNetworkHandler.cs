namespace FlowSystem
{
    internal interface IFlowNetworkHandler
    {
        bool AddSource(IFlowSource source);
        bool RemoveSource(IFlowSource source);

        bool AddSink(IFlowSink sink);
        bool RemoveSink(IFlowSink sink);

        bool AddBuffer(IFlowBuffer buffer);
        bool RemoveBuffer(IFlowBuffer buffer);
    }
}