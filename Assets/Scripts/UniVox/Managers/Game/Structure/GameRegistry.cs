using Unity.Collections;
using UnityEngine;
using UniVox.Launcher;
using UniVox.Managers.Game.Accessor;
using UniVox.Managers.Game.Native;
using UniVox.Managers.Generic;
using UniVox.Types;

namespace UniVox.Managers.Game.Structure
{
    public class MeshRegistry : BaseRegistry<MeshKey, MeshIdentity, Mesh>
    {
        protected override MeshIdentity CreateId(int index) => new MeshIdentity(index);

        protected override int GetIndex(MeshIdentity identity) => identity;
    }

    public class MaterialRegistry : BaseRegistry<MaterialKey, MaterialIdentity, Material>
    {
        protected override MaterialIdentity CreateId(int index) => new MaterialIdentity(index);

        protected override int GetIndex(MaterialIdentity identity) => identity;
    }

    public class SpriteRegistry : BaseRegistry<SpriteKey, SpriteIdentity, Sprite>
    {
        protected override SpriteIdentity CreateId(int index) => new SpriteIdentity(index);

        protected override int GetIndex(SpriteIdentity identity) => identity;
    }

    public class BlockRegistry : BaseRegistry<BlockKey, BlockIdentity, BaseBlockReference>
    {
        protected override BlockIdentity CreateId(int index) => new BlockIdentity(index);

        protected override int GetIndex(BlockIdentity identity) => identity;

        public NativeHashMap<BlockIdentity, NativeBaseBlockReference> CreateNative(
            Allocator allocator = Allocator.Persistent)
        {
            var map = new NativeHashMap<BlockIdentity, NativeBaseBlockReference>(Count, Allocator.Persistent);
            foreach (var pair in IdentityMap) map[pair.Key] = pair.Value.GetNative();
            return map;
        }
    }


    public class GameRegistry
    {
        public GameRegistry()
        {
            Meshes = new MeshRegistry();
            Materials = new MaterialRegistry();
            Sprites = new SpriteRegistry();
            Blocks = new BlockRegistry();

//            Raw = new ModRegistry();
//            Mods = new ModRegistryAccessor(Raw);
//            ArrayMaterials = new ArrayMaterialRegistryAccessor(Mods);
//            Icons = new IconRegistryAccessor(Mods);
//            Meshes = new MeshRegistryAccessor(Mods);
//            Blocks = new BlockRegistryAccessor(Mods); //,this);
//            SubArrayMaterials = new SubArrayMaterialRegistryAccessor(ArrayMaterials);
////            NativeBlocks = new NativeHashMap<BlockIdentity, NativeBaseBlockReference>(0, Allocator.Persistent);
//            Icons = new IconRegistryAccessor(Mods);
        }


        public MeshRegistry Meshes { get; }
        public MaterialRegistry Materials { get; }

        public SpriteRegistry Sprites { get; }
        public BlockRegistry Blocks { get; }

        /// <summary>
        ///     Adding blocks REQUIRES a Native UPDATE
        /// </summary>
//        public ModRegistry Raw { get; }

//        public ModRegistryAccessor Mods { get; }
//        public ArrayMaterialRegistryAccessor ArrayMaterials { get; }
//        public SubArrayMaterialRegistryAccessor SubArrayMaterials { get; }
//        public MeshRegistryAccessor Meshes { get; }

//        public BlockRegistryAccessor Blocks { get; }

//        public IconRegistryAccessor Icons { get; }
        public NativeHashMap<BlockIdentity, NativeBaseBlockReference> GetNativeBlocks()
        {
            return Blocks.CreateNativeBlockMap();
        }

//        public NativeHashMap<BlockIdentity, NativeBaseBlockReference> NativeBlocks { get; private set; }

//        public void UpdateNativeBlock()
//        {
//            NativeBlocks.DisposeEnumerable();
//            NativeBlocks = Blocks.CreateNativeBlockMap();
//        }
    }
}