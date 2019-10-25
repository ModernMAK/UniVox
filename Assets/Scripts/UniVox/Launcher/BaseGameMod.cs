using UniVox.Managers.Game.Accessor;
using UniVox.Types;

namespace UniVox.Launcher
{
    public class BaseGameMod : AbstractMod
    {
        public const string ModPath = "BaseGame";


        private const string GrassDirtMatPath = "Grass Dirt";
        private const string GrassSubMatPath = "Grass";
        private const string GrassSideSubMatPath = "Side";
        private const string DirtSubMatPath = "Dirt";


        private const string StoneMatPath = "Stone";
        private const string SandMatPath = "Sand";


        private const string GrassBlockPath = "Grass";
        private const string DirtBlockPath = "Dirt";
        private const string SandBlockPath = "Sand";
        private const string StoneBlockPath = "Stone";

        public static readonly MaterialKey GrassDirtMaterialKey = new MaterialKey(ModPath, GrassDirtMatPath);

        public static readonly SubArrayMaterialKey GrassSubMaterialKey =
            new SubArrayMaterialKey(GrassDirtMaterialKey, GrassSubMatPath);

        public static readonly SubArrayMaterialKey GrassSideSubMaterialKey =
            new SubArrayMaterialKey(GrassDirtMaterialKey, GrassSideSubMatPath);

        public static readonly SubArrayMaterialKey DirtSubMaterialKey =
            new SubArrayMaterialKey(GrassDirtMaterialKey, DirtSubMatPath);

        public static readonly MaterialKey StoneMaterialKey = new MaterialKey(ModPath, StoneMatPath);
        public static readonly MaterialKey SandMaterialKey = new MaterialKey(ModPath, SandMatPath);
        public static readonly BlockKey GrassBlock = new BlockKey(ModPath, GrassBlockPath);
        public static readonly BlockKey DirtBlock = new BlockKey(ModPath, DirtBlockPath);
        public static readonly BlockKey SandBlock = new BlockKey(ModPath, SandBlockPath);
        public static readonly BlockKey StoneBlock = new BlockKey(ModPath, StoneBlockPath);


        public override void Initialize(ModInitializer initializer)
        {
//            var modId = initializer.Registry.Mods.Register(ModPath);
////            var modRegistry = modId.Value;
//            var surrogate = ModResources.Load<ModSurrogate>(ModPath, "").Values;
//
//            //For now we use a proxy to register
//            //Should actually consider following Unity Patterns instead of this.
////            initializer.Registry.Mods.RegistrySurrogates(surrogate);
//
//            initializer.Registry.Raw[modId].RegistrySurrogates(surrogate);
//
//            var matReg = initializer.Registry.ArrayMaterials;
//            var subMatReg = initializer.Registry.SubArrayMaterials;
//            var blockReg = initializer.Registry.Blocks;
//
//            //YEah, this is a cluster, need to think of a better way to orgnaize data
//            if (!matReg.TryGetIdentity(GrassDirtMaterialKey, out var grassDirtMaterialId))
//                throw new AssetNotFoundException(nameof(GrassDirtMaterialKey), GrassDirtMaterialKey.ToString());
//
//            if (!subMatReg.TryGetValue(GrassSubMaterialKey, out var grassSubMatIndex))
//                throw new AssetNotFoundException(nameof(GrassSubMaterialKey), grassSubMatIndex.ToString());
//            if (!subMatReg.TryGetValue(GrassSideSubMaterialKey, out var sideSubMatIndex))
//                throw new AssetNotFoundException(nameof(GrassSideSubMaterialKey), sideSubMatIndex.ToString());
//            if (!subMatReg.TryGetValue(DirtSubMaterialKey, out var dirtSubMatIndex))
//                throw new AssetNotFoundException(nameof(DirtSubMaterialKey), dirtSubMatIndex.ToString());
//
//            if (!matReg.TryGetIdentity(StoneMaterialKey, out var stoneMaterialId))
//                throw new AssetNotFoundException(nameof(StoneMaterialKey), stoneMaterialId.ToString());
//            if (!matReg.TryGetIdentity(SandMaterialKey, out var sandMaterialId))
//                throw new AssetNotFoundException(nameof(SandMaterialKey), sandMaterialId.ToString());
            ;
//            blockReg.Register(GrassBlock,
//                new TopSideBlockRef(grassDirtMaterialId, grassSubMatIndex, sideSubMatIndex, dirtSubMatIndex));
//            blockReg.Register(DirtBlock, new RegularBlockRef(grassDirtMaterialId, dirtSubMatIndex));
//            blockReg.Register(StoneBlock, new RegularBlockRef(stoneMaterialId));
//            blockReg.Register(SandBlock, new RegularBlockRef(sandMaterialId));
        }
    }
}