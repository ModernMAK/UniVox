using System;
using Unity.Collections;
using UniVox.Launcher;
using UniVox.Managers.Game.Accessor;
using UniVox.Managers.Game.Native;
using UniVox.Managers.Game.Structure;
using UniVox.Types.Identities;
using UniVox.VoxelData;

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
        public NativeGameRegistry()
        {
            Blocks = new NativeHashMap<BlockIdentity, NativeBaseBlockReference>(0, Allocator.Persistent);
        }

        public NativeHashMap<BlockIdentity, NativeBaseBlockReference> Blocks { get; private set; }

        public void UpdateBlocksFromRegistry(BlockRegistryAccessor accessor)
        {
            Blocks.Dispose();
            Blocks = accessor.CreateNativeBlockMap();
        }

        private bool dispose;

        public void Dispose()
        {
            if (dispose)
                return;

            dispose = true;
            Blocks.Dispose();
        }
    }
}