using System;
using System.Collections.Generic;
using Types;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Asset_Management;
using UniVox.Core.Types;
using UniVox.Entities.Systems.Registry;
using UniVox.Entities.Systems.Surrogate;
using UniVox.Launcher;
using UniVox.Rendering.ChunkGen.Jobs;
using UniVox.Types;

namespace UniVox.Entities.Systems
{
    public class BaseGameMod : AbstractMod
    {
        public const string ModPath = "BaseGame";

        private const string GrassDirtPath = "Grass Dirt";

        public override void Initialize(ModInitializer initializer)
        {
            var modReference = initializer.Registries.Register(ModPath);
            var modRegistry = modReference.Value;
            var surrogate = ModResources.Load<ModSurrogate>(ModPath, "").Values;

            //For now we use a proxy to register
            //Should actually consider following Unity Patterns instead of this.
            modRegistry.RegistrySurrogates(surrogate);


            //YEah, this is a cluster, need to think of a better way to orgnaize data
            if (!modRegistry.Materials.TryGetReference("DirtGrass", out var matReference))
                throw new Exception("Asset not found!");
            var matId = new MaterialId(modReference.Id, matReference.Id);
            var matReg = matReference.Value;

            if (!matReg.SubMaterials.TryGetReference("Grass", out var grassRef))
                throw new Exception("Asset not found!");
            if (!matReg.SubMaterials.TryGetReference("Side", out var sideRef))
                throw new Exception("Asset not found!");
            if (!matReg.SubMaterials.TryGetReference("Dirt", out var dirtRef))
                throw new Exception("Asset not found!");

            ;
            modRegistry.Blocks.Register("Grass", new GrassBlockRef(matId, grassRef.Id, sideRef.Id, dirtRef.Id));


            modRegistry.Blocks.Register("Dirt", new RegularAtlasBlockRef(matId, dirtRef.Id));

            if (!modRegistry.Materials.TryGetReference("Stone", out var stoneReference))
                throw new Exception("Asset not found!");
            var stoneId = new MaterialId(modReference.Id, stoneReference.Id);
            modRegistry.Blocks.Register("Stone", new RegularBlockRef(stoneId));


            if (!modRegistry.Materials.TryGetReference("Sand", out var sandReference))
                throw new Exception("Asset not found!");
            var sandId = new MaterialId(modReference.Id, sandReference.Id);
            modRegistry.Blocks.Register("Sand", new RegularBlockRef(sandId));
//            modRegistry.Atlases.Register("Grass",);
        }

        public class RegularBlockRef : BaseBlockReference
        {
            private static readonly int _defaultSubMat = 0; //Rect _fullRect = new Rect(0, 0, 1, 1);

            public RegularBlockRef(MaterialId materialId)
            {
                _material = materialId;
            }

            private readonly MaterialId _material;


            public override void RenderPass(VoxelInfoArray.Accessor blockData, VoxelRenderInfoArray.Accessor renderData)
            {
                renderData.Material = _material;

                renderData.SetSubMaterial(Direction.Up, _defaultSubMat);

                renderData.SetSubMaterial(Direction.Down, _defaultSubMat);

                renderData.SetSubMaterial(Direction.Left, _defaultSubMat);
                renderData.SetSubMaterial(Direction.Right, _defaultSubMat);
                renderData.SetSubMaterial(Direction.Forward, _defaultSubMat);
                renderData.SetSubMaterial(Direction.Backward, _defaultSubMat);

//                renderData.Version.Dirty();
            }
        }

        public class RegularAtlasBlockRef : BaseBlockReference
        {
            public RegularAtlasBlockRef(MaterialId materialId, int subMat)
            {
                _material = materialId;
                _subMat = subMat;
            }

            private readonly MaterialId _material;
            private readonly int _subMat;


            public override void RenderPass(VoxelInfoArray.Accessor blockData, VoxelRenderInfoArray.Accessor renderData)
            {
                renderData.Material = _material;


                renderData.SetSubMaterial(Direction.Up, _subMat);

                renderData.SetSubMaterial(Direction.Down, _subMat);

                renderData.SetSubMaterial(Direction.Left, _subMat);
                renderData.SetSubMaterial(Direction.Right, _subMat);
                renderData.SetSubMaterial(Direction.Forward, _subMat);
                renderData.SetSubMaterial(Direction.Backward, _subMat);

//                renderData.Version.Dirty();
            }
        }

        public class GrassBlockRef : BaseBlockReference
        {
            public GrassBlockRef(MaterialId materialId, int grass, int sideSub, int dirtSub)
            {
                _material = materialId;
                _grassSubMat = grass;
                _grassSideSubMat = sideSub;
                _dirtSubMat = dirtSub;
            }

            private readonly MaterialId _material;

            //Cache to avoid dictionary lookups
            private readonly int _grassSubMat;
            private readonly int _grassSideSubMat;
            private readonly int _dirtSubMat;

            public override void RenderPass(VoxelInfoArray.Accessor blockData, VoxelRenderInfoArray.Accessor renderData)
            {
                renderData.Material = _material;

                renderData.SetSubMaterial(Direction.Up, _grassSubMat);

                renderData.SetSubMaterial(Direction.Down, _dirtSubMat);

                renderData.SetSubMaterial(Direction.Left, _grassSideSubMat);
                renderData.SetSubMaterial(Direction.Right, _grassSideSubMat);
                renderData.SetSubMaterial(Direction.Forward, _grassSideSubMat);
                renderData.SetSubMaterial(Direction.Backward, _grassSideSubMat);

//                renderData.Version.Dirty();
            }
        }
    }

    public abstract class BaseBlockReference
    {
        public abstract void RenderPass(VoxelInfoArray.Accessor blockData, VoxelRenderInfoArray.Accessor renderData);
        public void RenderPass(Chunk.Accessor chunk) => RenderPass(chunk.Info, chunk.Render);
    }

    public static class SurrogateProxyModHelpers
    {
        public static void RegistrySurrogates(this ModRegistry.Record modRegistry, ModRegistryRecordSurrogate surrogate)
        {
            foreach (var matSur in surrogate.Atlas)
            {
                var mat = matSur.Value;
                var regions = matSur.Regions;

                var atlasMaterialRef = modRegistry.Atlases.Register(matSur.Name, mat);
                foreach (var region in regions)
                {
                    atlasMaterialRef.Value.Regions.Register(region.Name, region.Value);
                }
            }

            foreach (var matSur in surrogate.Materials)
            {
                var mat = matSur.Value;
                var subMats = matSur.SubMaterials;

                var arrayMaterialRef = modRegistry.Materials.Register(matSur.Name, mat);
                foreach (var subMat in subMats)
                {
                    arrayMaterialRef.Value.SubMaterials.Register(subMat.Name, subMat.Value);
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