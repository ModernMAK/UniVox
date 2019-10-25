using System;
using Unity.Collections;
using UniVox.Launcher;
using UniVox.Types;

namespace UniVox.Managers
{
    public class NativeGameRegistry : IDisposable
    {
        private bool _dispose;

        public NativeGameRegistry()
        {
            Blocks = new NativeHashMap<BlockIdentity, NativeBlock>(0, Allocator.Persistent);
        }

        public NativeHashMap<BlockIdentity, NativeBlock> Blocks { get; private set; }

        public void Dispose()
        {
            if (_dispose)
                return;

            _dispose = true;
            Blocks.Dispose();
        }

        public void UpdateBlocksFromRegistry(BlockRegistry accessor)
        {
            Blocks.Dispose();
            Blocks = accessor.CreateNative(Allocator.Persistent);
        }
    }
}