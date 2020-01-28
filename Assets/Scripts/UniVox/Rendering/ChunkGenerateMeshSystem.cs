using Unity.Entities;
using Unity.Jobs;

namespace UniVox.Rendering
{
    namespace Rewrite
    {
        /// <summary>
        /// Helper functions for writing to native arrays/lists for meshing
        /// </summary>
        [AlwaysUpdateSystem]
        [UpdateInGroup(typeof(PresentationSystemGroup))]
        public class ChunkGenerateMeshSystem : JobComponentSystem
        {
            protected override JobHandle OnUpdate(JobHandle inputDeps)
            {
                //Gather changed chunks
                //Calculate unique batches (unique blockIds, could be stored in a chunkside component)
                //PER BATCH
                //    Create a mesh buffer
                //    Pass buffer and render information to proxy
                //    Add handle to dependency, 

                //.....

                //Iterate over incomplete handles
                //IF COMPLETE
                //    Convert buffer to mesh
                //TODO
//                throw new System.NotImplementedException();
                return inputDeps;
            }
        }
    }
}