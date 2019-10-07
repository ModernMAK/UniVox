

I accidentally tried loading in 21x21x21 chunks, and hit my memory cieling (12 Gig), so I figured id write up some math to guess why this is.


10 chunks in each direction is 21 cunks per axis (and 320 blocks in one direction)

With 21^3 chunks, we have ~10K chunks loading at one time. (That is per PLAYER [more accurately, chunk streaming target]). And each chunk has ~32K voxels. Each voxel has (as of me writing this) an id (5 bytes), culling Flag (1 byte), an activeFlag (1 byte), a materialId (5 bytes), a subMaterialID (24 bytes), and a Shape (1 byte). For a total of 37 bytes. (Each chunk also has some additional overhead,  not per block)

Therfore, we are using 10K * 32K * 37 bytes.
Turns out that is roughly 12 Gig, MY MEMORY Cieling! How do we stop this?
We could drop subMatId to less bytes, we need 6 pieces of information, which we could limit to a byte, reducing our total to 19. That brings us down to 6.08 Gig. Better but not really 'good'.

Well certain information is rendering only, at the cost of time (which we might be able to get away with, mesh gen is still unoptimized, so a change like this will probably be negligable for now) 
MatId, SubMatId, and CullingFlag are all render only information (Shape is special, since it is rendering information, but can't be determined by anything else) that brings us down to 7 bytes.

That is 2.24 Gig...

While writing this, I realized a naive mistake I'd made. Unless specified otherwise, most types are backed as ints.
This is only relevant for the activeFlag, which is actually 4 bytes. That pushes our data usage to 3.2 Gig


Unfortunately, we cant merge 'activeFlag' into it's neighbors. (We could but then we may have to work on batches of voxels. This isn't bad, but i've tried it and it can be problematic).

A Recap, after dropping render info, we are at ID (5 bytes), Active (4 bytes), Shape (1 byte) - 10 bytes


We could use a short for blockId, and a lookup per chunk. This unfortunately, requires us to cache this lookup table somewhere.

Assuming we only need these four pieces of information, and we solve the lookup table problem, that is 4 bytes (one int!) That brings us to 1.2 Gig, pretty good.

But can we do better?

Chances are we only want to see 10 chunks away, and keep the chunks loaded if they are 2 chunks away.

After rendering our chunk, we save the chunk and keep the mesh in memory (GPU-bound).

Working with 5^3 chunks is 125 (1/8th), with ~32K voxels and our int representation of a voxel.
125 * 32K * 4 is 16 Megabytes of memory. Our meshes use roughly 255 MB (if i remember correctly, they are 2.04MB, but I could be wrong). So this would be 'Our Best' we could optimize further but it'd be better to optimize meshing than deal with this.