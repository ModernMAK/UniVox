using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityEdits.Rendering
{
    /// <summary>
    /// Renders all Entities containing both RenderMesh & LocalToWorld components.
    /// </summary>
    [ExecuteAlways]
    [DisableAutoCreation]
    //@TODO: Necessary due to empty component group. When Component group and archetype chunks are unified this should be removed
    [AlwaysUpdateSystem]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
//    [UpdateAfter(typeof(LodRequirementsUpdateSystemV3))]
    public class RenderMeshSystemV4 : JobComponentSystem
    {
        EntityQuery _meshSystem;

        protected override void OnCreate()
        {
            //@TODO: Support SetFilter with EntityQueryDesc syntax

            //We setup a DontRenderTag, which excludes all entites that dont want to be rendered but have the tag


            _meshSystem = GetEntityQuery(
                ComponentType.ReadOnly<LocalToWorld>(),
                ComponentType.ReadOnly<RenderMesh>(),
                ComponentType.Exclude<DontRenderTag>()
            );
        }

//        protected override void OnDestroy()
//        {
//        }
//

        void DrawMesh(ArchetypeChunk chunk)
        {
            var localToWorld = chunk.GetNativeArray(GetArchetypeChunkComponentType<LocalToWorld>(true));
            var localToWorldArray = new Matrix4x4[localToWorld.Length];
            for (var i = 0; i < localToWorld.Length; i++)
                localToWorldArray[i] = localToWorld[i].Value;

            var renderMesh =
                chunk.GetSharedComponentData(GetArchetypeChunkSharedComponentType<RenderMesh>(),
                    EntityManager);

            Graphics.DrawMeshInstanced(renderMesh.mesh, renderMesh.subMesh, renderMesh.material, localToWorldArray,
                localToWorld.Length, default, renderMesh.castShadows, renderMesh.receiveShadows, renderMesh.layer,
                default);

        }


        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            inputDeps.Complete(); // #todo

            var chunks = _meshSystem.CreateArchetypeChunkArray(Allocator.TempJob);

            Profiler.BeginSample("Batch Chunk");
            for (var i = 0; i < chunks.Length; i++)
            {
                Profiler.BeginSample("Draw Chunk");
                DrawMesh(chunks[i]);
                Profiler.EndSample();
            }
            Profiler.EndSample();
            
            chunks.Dispose();

            return new JobHandle();
        }
    }
}