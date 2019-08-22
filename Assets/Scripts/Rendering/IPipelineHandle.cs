using System;
using Unity.Jobs;

namespace Rendering
{
    public interface IPipelineHandle : IDisposable
    {
        JobHandle Handle { get; }
    }
}