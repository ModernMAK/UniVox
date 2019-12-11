Rather than focusing and fixing things im going to brainstorm. Yay procrastination.

Lets talk tools.
Minecraft: Shovel, Hoe, Axe, Pickaxe
Terraria: Shovel, Pickaxe, Axe, Hammer
Dragon Quest: Baton, Hammer, 
Autonauts: Shovel, Hoe, Axe, Scythe, Shears

Regardless of how many tools.....
-----

Lets talk about my thoughts and revisions to rendering.
Currently I have 'all blocks render the same' vs my expected 'blocks handle their own rendering'. I say this, knowing full well that the first thing I will be doing is implimenting a proxy-class which can handle most blocks.

I think this is neccessary, as it makes it easier to alter the block-rendering pipeline; either from the top  (Handling chunks) or the bottom (handling voxels). To elaborate on some of the benefits, blocks can interact with render data differently; going so far as adding extra verticies or supplying neccesary information to the material/shader for the given block.

Some things to consider; blockdata, lighting, and new render data.


As mentioned in a 0fps post (off of memory, need to fact-check) lighting in minecraft is 2 channels. Natural and artificial light. They expand on this; RGB and natural. I don't recall what their algorithm was and I feel thats a blessing in disguise as I experiment.

Some thoughts on lighting; lights have color, range, and attenuation(?) in this model light travels the full range, but attenuation alters its strength as it travels.

Alternatively, color, strength, and range; similair to attenuation, however strength decreases with every block traveled. if strength is greater than the light-size, than it uses the maximum allowed.

Regardless of how lighting is handled, the general idea is that voxels have some lighting data, and that it should be used to determine a vertex's lighting.


I'd prefer if blockdata was simply the block identity, but this brings us other problems.
First and foremost; placing blocks. This was easy when we could check if the block was active, but now we'd actively need to check against an 'Air Block' of some kind.
Second, specialty data needs to be stored somewhere, my plan was to move this data to the heap, since not every block needs to support things like block shapes.

It seems that blockdata now needs to know 3 things, two per block (ID, Active) and one per chunk, Metadata Table.

Lastly, the new Render Data. since blocks handle their own render pipelines, we need to think about what needs to change here.

Neighbor culling should still be enforced, although I should probably impliment a system that doesnt exclusively use the 'active' setting.

A different kind of render data is important, sub-batching. We can already batch by ID, but we probably want to be able to create sub batches for blocks that use metadata to change materials. This doesn't have to be implimented immediately but it's something to consider.

 