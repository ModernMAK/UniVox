using System;
using Unity.Collections;
using UniVox.Launcher;
using UniVox.Managers.Game.Accessor;
using UniVox.Managers.Game.Native;
using UniVox.Types.Identities;

namespace UniVox.Managers.Game.Structure
{
    public class GameRegistry
    {
        public GameRegistry()
        {
            Raw = new ModRegistry();
            Mods = new ModRegistryAccessor(Raw);
            ArrayMaterials = new ArrayMaterialRegistryAccessor(Mods);
            Meshes = new MeshRegistryAccessor(Mods);
            Blocks = new BlockRegistryAccessor(Mods); //,this);
            SubArrayMaterials = new SubArrayMaterialRegistryAccessor(ArrayMaterials);
//            NativeBlocks = new NativeHashMap<BlockIdentity, NativeBaseBlockReference>(0, Allocator.Persistent);
        }


        /// <summary>
        /// Adding blocks REQUIRES a Native UPDATE
        /// </summary>
        public ModRegistry Raw { get; }

        public ModRegistryAccessor Mods { get; }
        public ArrayMaterialRegistryAccessor ArrayMaterials { get; }
        public SubArrayMaterialRegistryAccessor SubArrayMaterials { get; }
        public MeshRegistryAccessor Meshes { get; }

        public BlockRegistryAccessor Blocks { get; }

        public NativeHashMap<BlockIdentity, NativeBaseBlockReference> GetNativeBlocks()
        {
            return Blocks.CreateNativeBlockMap();
        }

//        public NativeHashMap<BlockIdentity, NativeBaseBlockReference> NativeBlocks { get; private set; }

//        public void UpdateNativeBlock()
//        {
//            NativeBlocks.Dispose();
//            NativeBlocks = Blocks.CreateNativeBlockMap();
//        }
    }
}