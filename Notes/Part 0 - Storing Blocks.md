
# Storing Blocks
## Some Vocabulary
The terms Voxel and Block are interchangeable. I refer to Unity's Chunks as Partitions, to distinguish them from my Voxel Chunks.

## How Should We Store Them
Storing a Block is fairly simple, as all blocks are bound to have some similar information. This information should be the bare minimum knowledge a block needs to represent itself.

---
Partitions make grouping information easy... But we want to group information in two ways.

We want to group all entities that share information. Partitions do this naturally with Archetypes. But we also want to group entities by their Chunk, which requires a bit of work, and depends on whether we work at the Voxel or the Chunk level.

### Voxels vs Chunks
If we work at an entity level, we are creating a lot of overhead when creating a Chunk; 64 (2^3) entities with a byte, and a whopping  32768 (32^3) entities with a short!

Therfore, we should represent Chunks at the Voxel level.

---
Because we group entities by their archetypes, and each entity is a chunk, we can group voxels by chunk and by their core data. 

### Some New Problems
With Chunk-Entities we can easily group information, but that information will be present for every block.

What if we want add some information that only a few blocks need?

We could do some trickery by using an Index Component and a Data Component, Allowing us to have a sparse array.

Or we could add it to every block and only use it for relevant blocks.


#### Making Matters Worse
The biggest problem however, is how do we manage these components?

When a BlockID changes we would want to add or remove these optional components. We could have them present all the time, but that just adds more bloat to the archetype.

#### My Preferred Solution
Every block has Block Info, and a Metadata ID, which can be used to get MetaData from some MetaData manager.

Block Info and the Metadata ID are part of the Partition, and Dynamic Metadata is be loaded in manually.

