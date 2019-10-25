using System;
using Unity.Collections;
using UniVox.Launcher;
using UniVox.Managers.Game.Accessor;
using UniVox.Managers.Game.Structure;
using UniVox.Types;

namespace UniVox
{
    public static class GameManager
    {
        public static readonly GameRegistry Registry = new GameRegistry();
        public static readonly NativeGameRegistry NativeRegistry = new NativeGameRegistry();
        public static readonly Universe Universe = new Universe();
    }

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