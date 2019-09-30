# Design 
A Voxel represents the core of the game. For all intents and purposes, a Block will store the Rendering Information.

A Block needs to know its worldspace position, & rotation, its 'Shape', 'Material' & whether it has been culled. *Therefore* a Block knows it's **Position**, **Rotation**, **Mesh**, **Material**, and whether it has been **Culled**.


# Chunk Streaming & Async Pipelines
## Chunk Creation
Chunks need to be valid when present in the World, a chunk is not valid until all steps of creation are completed.

Ergo, our Chunk manager needs to know that a chunk is invalid(unloaded), being created but not valid yet, or created and valid.

## Chunk Rendering
Chunk rendering requires that the data does not change between chunk analysis (to determine mesh size) and mesh generation (to avoid index out of range)

The two solutions I can think of include: working on copies and single frame rendering.

Copies allow us to use what we have with the added step of gathering and copying the correct information. This can be costly but may outway the slowdown of rendering in a single frame.

Alternatively we can render a chunk in a single frame, garunteeing that all information is synchronized properly. This will likely cause stutters. It should be mentioned that every chunk to be rendered doesnt have to be rendered in the same frame.

## Chunk Pipelines
With the two above problems addressed, one last thing to address is what is valid and when.
Perhaps a pipeline which uses flags to let us know what state chunks are in and work on them properly?
-> Creating | Valid | RequestingRender  

This couples our code though