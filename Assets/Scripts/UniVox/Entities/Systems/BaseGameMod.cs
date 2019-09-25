using System;
using System.Collections.Generic;
using Types;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Core.Types;
using UniVox.Entities.Systems.Registry;
using UniVox.Entities.Systems.Surrogate;

namespace UniVox.Entities.Systems
{
    public class BaseGameMod : AbstractMod
    {
        public const string ModPath = "BaseGame";

        private const string GrassDirtPath = "Grass Dirt";

        public override void Initialize(ModInitializer initializer)
        {
            var modRegistry = initializer.Registries.Register(ModPath, out int modId);
            var surrogate = ModResources.Load<ModSurrogate>(ModPath, "").Values;

            //For now we use a proxy to register
            //Should actually consider following Unity Patterns instead of this.
            modRegistry.RegistrySurrogates(surrogate);


            //YEah, this is a cluster, need to think of a better way to orgnaize data
            if (!modRegistry.Materials.TryGetIndex("DirtGrass", out var matIndex))
                throw new Exception("Asset not found!");
            if (!modRegistry.Materials[matIndex].Regions.TryGetValue("Grass", out var grassRect))
                throw new Exception("Asset not found!");
            if (!modRegistry.Materials[matIndex].Regions.TryGetValue("Side", out var sideRect))
                throw new Exception("Asset not found!");
            if (!modRegistry.Materials[matIndex].Regions.TryGetValue("Dirt", out var dirtRect))
                throw new Exception("Asset not found!");

            ;
            modRegistry.Blocks.Register("Grass",
                new BlockRegistryRecord(new GrassBlockRef(matIndex, grassRect, sideRect, dirtRect)));


            modRegistry.Blocks.Register("Dirt", new BlockRegistryRecord(new RegularAtlasBlockRef(matIndex, dirtRect)));

            if (!modRegistry.Materials.TryGetIndex("Stone", out var stoneIndex))
                throw new Exception("Asset not found!");
            modRegistry.Blocks.Register("Stone", new BlockRegistryRecord(new RegularBlockRef(stoneIndex)));


            if (!modRegistry.Materials.TryGetIndex("Sand", out var sandIndex))
                throw new Exception("Asset not found!");
            modRegistry.Blocks.Register("Sand", new BlockRegistryRecord(new RegularBlockRef(sandIndex)));
//            modRegistry.Materials.Register("Grass",);
        }

        public class RegularBlockRef : BaseBlockReference
        {
            static readonly Rect _fullRect = new Rect(0, 0, 1, 1);

            public RegularBlockRef(int materialId)
            {
                material = materialId;
            }

            private readonly int material;


            public override void RenderPass(VoxelInfoArray.Accessor blockData, VoxelRenderInfoArray.Accessor renderData)
            {
                renderData.Material = material;

                renderData.SetRegion(Direction.Up, _fullRect);

                renderData.SetRegion(Direction.Down, _fullRect);

                renderData.SetRegion(Direction.Left, _fullRect);
                renderData.SetRegion(Direction.Right, _fullRect);
                renderData.SetRegion(Direction.Forward, _fullRect);
                renderData.SetRegion(Direction.Backward, _fullRect);

//                renderData.Version.WriteTo();
            }
        }

        public class RegularAtlasBlockRef : BaseBlockReference
        {
            public RegularAtlasBlockRef(int materialId, Rect rect)
            {
                material = materialId;
                rect = rect;
            }

            private readonly int material;
            private readonly Rect rect;


            public override void RenderPass(VoxelInfoArray.Accessor blockData, VoxelRenderInfoArray.Accessor renderData)
            {
                renderData.Material = material;


                renderData.SetRegion(Direction.Up, rect);

                renderData.SetRegion(Direction.Down, rect);

                renderData.SetRegion(Direction.Left, rect);
                renderData.SetRegion(Direction.Right, rect);
                renderData.SetRegion(Direction.Forward, rect);
                renderData.SetRegion(Direction.Backward, rect);

//                renderData.Version.WriteTo();
            }
        }

        public class GrassBlockRef : BaseBlockReference
        {
            public GrassBlockRef(int materialId, Rect grass, Rect side, Rect dirt)
            {
                material = materialId;
                grassRect = grass;
                grassSideRect = side;
                dirtRect = dirt;
            }

            private readonly int material;

            //Cache to avoid dictionary lookups
            private readonly Rect grassRect;
            private readonly Rect grassSideRect;
            private readonly Rect dirtRect;

            public override void RenderPass(VoxelInfoArray.Accessor blockData, VoxelRenderInfoArray.Accessor renderData)
            {
                renderData.Material = material;

                renderData.SetRegion(Direction.Up, grassRect);

                renderData.SetRegion(Direction.Down, dirtRect);

                renderData.SetRegion(Direction.Left, grassSideRect);
                renderData.SetRegion(Direction.Right, grassSideRect);
                renderData.SetRegion(Direction.Forward, grassSideRect);
                renderData.SetRegion(Direction.Backward, grassSideRect);

//                renderData.Version.WriteTo();
            }
        }
    }

    public abstract class BaseBlockReference
    {
        public abstract void RenderPass(VoxelInfoArray.Accessor blockData, VoxelRenderInfoArray.Accessor renderData);
    }

    public static class SurrogateProxyModHelpers
    {
        public static void RegistrySurrogates(this ModRegistryRecord modRegistry, ModRegistryRecordSurrogate surrogate)
        {
            foreach (var matSur in surrogate.Materials)
            {
                var mat = matSur.Value.Value;
                var regions = matSur.Value.Regions;

                modRegistry.Materials.Register(matSur.Name, mat, out var matId);
                foreach (var region in regions)
                {
                    modRegistry.Materials[matId].Regions.Register(region.Name, region.Value);
                }
            }
        }

        public static bool TryGet<T, U>(this IList<T> blockReg, string name, out U record) where T : NamedValue<U>
        {
            for (var i = 0; i < blockReg.Count; i++)
            {
                var item = blockReg[i];
                if (item.Name.Equals(name))
                {
                    record = item.Value;
                    return true;
                }
            }

            record = default;
            return false;
        }
    }
}