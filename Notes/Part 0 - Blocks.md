To avoid confusion I redefine terms here
Voxel/Block - interchangable
Partition - Unity ECS's Chunk
Chunk - A Chunk of voxels, grouped by position




Storing Blocks and Storing Block Info

Storing a Block is fairly simple, all blocks are bound to have some similar information. This information is the bare minimum knowledge a block needs to represent itself. It has no special features whatsoever.

---
Partitions make grouping information easy... But we want to group information in two ways.

We want to group all entities that share information (which Partitions do), and  we want to group entities by their Chunk.

If we work at an entity level, we are creating a lot of overhead when creating a Chunks (64 entities with a byte, 32768 entities with a short)

Now that I've said this on paper, it makes sense how Chunks creation slows us down.

So..., we need to work at the Chunk Level.


So the core data per block can easily be stored in an entity, in this scenario, and Entity represents our Chunks, Chunks contain all their Blocks, and Chunks are grouped by partition. got all that?

Now we need a way to store metadata. We could keep this as a component on the entity as well, but we can't garuntee everyblock will have that metadata, so we need it to be sparse.

Ontop of that, Metadata will not affect every block, so it should probably be a sparsely populated.

Then of course, how do we manage chunk metadata, we would need to ensure that a chunk had certain metadata components when changing its block id.

We could use container class to cache/add/remove metadata components. A Metadata registry, which could use the block id...

We also probably dont want to allocate classes and should pack our structs/enums to as small as possible.

I suppose before I can work on storing metadata, I should find some examples... But instead, my though process.

Every block has Block Info, and a small bit of Static Metadata, then their is Dynamic Metadata

Block Info and Static Metadata are part of the chunk, and Dynamic Metadata, which is kept outside of the chunk and must be loaded manually.

Static can help with Simple things like Block Flags, or variants, whereas Dynamic lets you store anything like JSON




