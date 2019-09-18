# Memory Management
Oh mystical profiler... what do you say? Meshes are eating my memory? 2.4 MB a mesh? Well, thats not so bad... with 800 meshes? That makes sense... Wait, so im expecting (on a best case with 32-size chunks and seeing up to 8 chunks away) to chew through 8.1 Gigs!
How do we stop this!

## Vertex Layout (Skippable)
This doesn't seem to add up. Feel free to skip it.

This is a new feature and one im extremely unfamiliar with. So I probably wont do it. BUT,for fun, lets see how much we could save by reducing the size of the buffer itself.
| Element | Components  | Original Size | Orignal Bytes | New Size |  New Bytes | Gain |
|---|---|---|---|---|---|  ---| ---|
|Position|3|Float16|12 (4*3)|Float16|6 (2*3)| 6 |
|Normal|3|Float16|12 (4*3)|Float16|6 (2*3)|6|
|Tangent|4|Float16|16 (4*4)|Float16|8 (2*4)|8|
|Texture0|2|Float16|8 (4*2)|Float16|4 (2*2)|4|
|TOTAL|||48||24|24|

According to logic, a vertex has about 94 bytes, This does not seem right at all
2.4 Megabytes - (32*32*6*4 or) 24576 Vertexes - (32*32*6*6 or) 36864 Indexes
Indexes are shorts (or ints); 73728 Bytes or 147456 Bytes
that leaves us with 2.326272 MB and 2.252544 MB. So rougly 2.33 and 2.25 MB
Distributing vertexes gives us 94 bytes or 92 bytes. So 94 bytes then?

Something really doesn't feel right here. Leaving this here so I can learn from it.
## Improved Meshing Algorithms
[0FPS](https://0fps.net/2012/06/30/meshing-in-a-minecraft-game/) - Almost everything I talk about here in this analysis, I learned about by reading this article.

Currently, I dont draw hidden faces, this saves vertexes, and helped me ignore the 65K vertex limit when working with certain mesh sizes. 0FPS calls this Culled.

If I didn't cull hidden faces, I'd have to render every face, which is 24 vertexes per voxel. For 32x32x32 voxels, thats way more than our 65K limit (we actualaly only get 2 verts per voxel!) Calls this Naive.

To solve my memory problem, I have to get... Greedy. 0FPS's algorithm does something to merge adjacent quads. I don't recall specifics, but 0FPS worked at the plane level, this works well for Boxels, with only 6 planes to work with.

Our Special Boxels complicate this.
Instead of working with quads for the entire algorithm, we also work with triangles.
Instead of having 6 planes, we have ALOT. 
6 planes for the standard Boxel. 6 planes for the median (think minecraft slabs)
6 Planes for the ramps, and 8 planes for corners. That's 26 planes to consider. With 3 Basic shapes, Squares, Rectangles, and Triangles.

Despite having 26 some planes, most can be represented with 6 bits. 1 bit each for the 6 primary planes. But, because of the added complexity, we should keep our first draft to our standard cube, and work in our nice, safe 6 plane-quads.

### Greedy Implementation
We work on a Plane Basis, we intuitively know that we start at 0,0 on this grid plane, and we progress until we reach h,w. On this plane, each voxel knows whether it should be culled, what shape it is, and what texture it wants to use. We want to merge edges if these two are the same.

Knowing these facts, we simply have a frontier which tracks if we have processed that voxel, and a frontier which keeps track of our batched faces.
We begin by travelling up and merging faces, then, we attempt to move out, if moving out fails, we stop moving out and finalize our batch, telling the frontier that we have investigated these tiles.