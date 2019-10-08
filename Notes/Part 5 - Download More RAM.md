

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
125 * 32K * 4 is 16 Megabytes of memory. Our meshes use roughly 500 KB, as they are 4KB per mesh; 125*4 is 500KB. So can we go further? 4 bytes is our logical limit, if EVERY block is unique in the chunk, then it will require 15 bits for our ID, shape requires a byte, and our active flag requries 1 bit. We could pack active into the ID to get 3 bytes. (12 MB, an improvement?)

BUT WAIT, theres more!

Since we have already moved around a lot of data, what if we increase our chunk size? arbitrarily i wanted to keep it inside the isze of a short, but what if I made a chunk something ridiculous like 256^3

For one, we probably only need the current chunk and the neighboring chunk (256 out is a little shorter than our 320, but our effective distance, from the center block of our center chunk (384 is greather than the effective 336)
so we only load 1 chunk away
9 * (256^3) * 4 unsurprisingly, we've reached 600 MB. Let's try 25 * (128) * 4... without calculating thats even greater (1.8 Gig for those curious. Also thats 320, roughly our old effective base so we did worse)

The memory formula im using. (B) * (1 + 2D) * (A^3) wher B is bytes per voxel, D is the distance outwards in chunks, and A is the axis size of the chunk (voxels per axis).

I think the best we can get is 128 loading one chunk away, but i will stick to 32 sized chunks for now; as increasing the chunk size increases the number of bytes we need for the unique block index (although anything greater than 32 means we need an int, and we could pack the extra bits with something else, meaning worst case we'd probably need 5 bytes)


Despite everything I said, baby steps. First I'll strip non-critical information. Further optimizations can come later.
