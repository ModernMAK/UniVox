using System;
using Types;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Asset_Management;
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
            var modId = initializer.Registry.Mods.Register(ModPath);
//            var modRegistry = modId.Value;
            var surrogate = ModResources.Load<ModSurrogate>(ModPath, "").Values;

            //For now we use a proxy to register
            //Should actually consider following Unity Patterns instead of this.
//            initializer.Registry.Mods.RegistrySurrogates(surrogate);


            //YEah, this is a cluster, need to think of a better way to orgnaize data
            var dirtGrassKey = new ArrayMaterialKey()
            if (!initializer.Registry.ArrayMaterials.TryGetIdentity("DirtGrass", out var matReference))
                throw new Exception("Asset not found!");
            var matId = new MaterialId(modId.Id, matReference.Id);
            var matReg = matReference.Value;

            if (!matReg.SubMaterials.TryGetReference("Grass", out var grassRef))
                throw new Exception("Asset not found!");
            if (!matReg.SubMaterials.TryGetReference("Side", out var sideRef))
                throw new Exception("Asset not found!");
            if (!matReg.SubMaterials.TryGetReference("Dirt", out var dirtRef))
                throw new Exception("Asset not found!");

            ;
            modRegistry.Blocks.Register("Grass", new GrassBlockRef(matId, grassRef.Id, sideRef.Id, dirtRef.Id));


            modRegistry.Blocks.Register("Dirt", new RegularBlockRef(matId, dirtRef.Id));

            if (!modRegistry.Materials.TryGetReference("Stone", out var stoneReference))
                throw new Exception("Asset not found!");
            var stoneId = new MaterialId(modId.Id, stoneReference.Id);
            modRegistry.Blocks.Register("Stone", new RegularBlockRef(stoneId));


            if (!modRegistry.Materials.TryGetReference("Sand", out var sandReference))
                throw new Exception("Asset not found!");
            var sandId = new MaterialId(modId.Id, sandReference.Id);
            modRegistry.Blocks.Register("Sand", new RegularBlockRef(sandId));
//            modRegistry.Atlases.Register("Grass",);
        }
    }
}