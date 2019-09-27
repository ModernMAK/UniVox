

# Rendering Revisions
Alright, so from revising my rendering code for the umpteenth time. I've been tossing around many ideas on how I planned to do rendering.


## First Attempt : Atlases
Originally, I was using atlases, with separate Atlases for different purposes to avoid bleeding. This was represented by RenderModeFlags, with the two relevant flags copied here: Transparent, and Emissive. Transparent allowed us to avoid transparency bleeding, Emissive allowed us to avoid emission bleeding onto non emissive textures. This would mean the end product has 4 atlases. This would work in conjunction with the Boxel Mesh, which would generate UV information for each block and separate into the 4 meshes per atlas.

##Second Attempt : Voxel As Entities
When I Moved to ECS, I tried the Voxel-As-Entity; which to summarize, was too expensive and did not work.

My rendering did however move to 'Render Groups' which were batched Meshes and Materials which were merged together. Merging these meshes removed our ability to use batch groups, and in retrospect, I think this is acceptable ONLY for Boxel Meshes (More on this after Chunk-As-Entity).

## Third Attempt : Chunk As Entity (Current)
In Chunk-As-Entity rendering; we also work with 'Render Groups; but focused only on the Material. Meshes are generated using a BlockShape and the Visible Faces, which allows us to cull excess vertexes in the merged mesh. This removes our ability to use custom meshes however.

With merged meshes, we get the benefit of one draw call at the cost of not being able to batch. In layman's, we get the benefit of doing one thing, at the cost of not being able to do many things as one thing. For a merged Boxel Mesh, this is fine, since chunks are effectively unique (since we can't efficiently determine when they aren't). 

## Looking To The Future
### Using An Atlas
We should still use an atlas, as we now have an extra draw call PER unique material PER Chunk. E.G. Assuming we work with short Chunks (32x32x32) and all blocks in a Chunks are unique, and we have 9^3 chunks loaded. That's a whopping 23,887,872 Draw calls. Roughly 24 Million draw calls. That's Insane! Granted this is a worst case scenario, which uses some trickery with transparent blocks to force all blocks in a chunk to be rendered.

To circumvent this, we can do two things; using an Atlas, and Chunk Culling (in addition to our current Voxel-Face Culling). Someone at Minecraft wrote a great article about storing a Chunk's Face to Face visibility, which only needs a short to store (technically less, I think 15 bits, one per combination). While that can help, in our worst case scenario, it wouldn't change anything. So we need atlases.

Both modded Minecraft & modded Factorio (to my knowledge) generate Atlases at runtime. This makes for longer load times, with the benefit of less draw calls. Which is why I'd like to try Per-Mod Atlases. This would allow Atlases to be pre-generated and packed for distribution, or generated once and then cached. I can see this running into problems like adding or replacing textures is more difficult, but the impossibility of having 24 Million Mods, (or 32768 for that matter) means we will have way less draw calls in our worst case. Assuming a theoretical mod limit of 512 (going off my own experience with Minecraft Forged), we now have 512*(9^3) for 373,248 or about 400 thousand. That's 60 Times better. And that's only our worst case!

Of course, if we merged all Atlases, our worst case becomes our best case, with no unique Materials; we only worry about the unique meshes draw call, which leaves us with 9^3, or 729. We cannot do better without batching chunks or throwing away hidden or empty chunks. (Throwing away hidden chunks may be possible per the Chunk Face Visibility Method).

In our best case scenario, regardless of chunk size, we have 729 Draw Calls, one per chunk in our 9^3 render distance. (Fun Fact Time; Assuming chunks are 32 blocks, and we are in the center, that's 128 blocks in any direction). 

##### Draw Calls Per Chunk
| Case | Unique Material  | Mod Atlas | Global Atlas |
| ------------ | ------------ | ------------ | ------------ |
|  Best Case  | 1 | 1 | 1 |
| Worst Case |  32,768 | 512 | 1 |
| Guesstimate Case | 256 | 16 | 1 |

##### Draw Calls when loading 729 (9^3) Chunks
| Case | Unique Material  | Mod Atlas | Global Atlas |
| ------------ | ------------ | ------------ | ------------ |
|  Best Case  | 729 | 729 | 729 |
| Worst Case |  ~24 M | ~373 K| 729 |
| Guesstimate Case | ~187 K| 11,664 | 729 |

Guesstimate is my estimate of how many will be average, I assume 256 Blocks


#### But what about Texture Bleeding!
Remember how in the First Attempt we had 4 atlases? Doesn't that increase our Draw Calls? Well, yeah, it does.


##### Draw Calls Per Chunk with 4 Atlases
| Case | Unique Material  | Mod Atlas | Global Atlas |
| ------------ | ------------ | ------------ | ------------ |
|  Best Case  | 4 | 4 | 4 |
| Worst Case |  ~131K | 2,048 | 4 |
| Guesstimate Case | 1,024 | 1,024 | 4 |

##### Draw Calls when loading 729 (9^3) Chunks with 4 Atlases
| Case | Unique Material  | Mod Atlas | Global Atlas |
| ------------ | ------------ | ------------ | ------------ |
|  Best Case  | 729 | 729 | 729 |
| Worst Case |  ~24 M | ~1.5 M | 2,916 |
| Guesstimate Case | ~373 K| 23,328| 1,458 |

Guesstimate is my estimate of how many will be average, I assume 256 Blocks



# Conclusion On Materials, Atlases, and Benefits
When I reviewed entities, I went from ~32K Entities, to one entity with a possibility of ~32K Entities (Currently using Unique Material)

After writing this analysis, an Atlas is vital. Not only does it remove the overhead of 40x draw calls in the worst case (all 8000x while indev; using only one atlas) but removes the overhead of creating ~32K Entities; replacing it with only 2K Entities, (16x decrease!).

Also, since it's not obvious, entities (or Unity's GameObjects, but I use entities) are required because we need Physics. Otherwise I could just render to camera using the API and avoid Entity Creation.

##### Draw Calls Compared To Base
Base has least # of Draw Calls
Assumes Chunks are 32x32x32, we have 4 Atlases, and a 512 Mod Limit
| Case | Unique Material  | Mod Atlas | Global Atlas |
| ------------ | ------------ | ------------ | ------------ |
|  Best Case  | Base | Base | Base |
| Worst Case |  ~8000x | ~500x | Base |
| Guesstimate Case | ~250x | ~15x | Base |


# Some Obvious Problems
Some Red Flags after reading this are, Custom Meshes for blocks. E.G. A chair or painting. 
## Custom Meshes On The Grid
The original plan was to have Blocks, stored as Voxels. And Voxel Entities (hereon VoxEntities to avoid confusing with Unity's Entities), which were free and independent from Voxels. If we wanted something to be on the Voxel Grid, but use a custom mesh, it would have to be a VoxEntity. Is there a problem with this?
No. I don't think so. From a rendering perspective, this is perfectly fine VoxEntities have the benefit of being batched, and can then avoid the use of an Atlas. Whether they should is up to a completely separate analysis article.
## Custom Meshes Off The Grid
As for things that aren't on the grid, same rules apply. Only now we don't need to specify rules for the Voxel Grid.
## Custom Materials & Shaders?
Using custom materials is just bad (under my proposed method). Using a separate material would require reusing an atlas, OR creating a separate atlas. For the purpose of the analysis above, we assume that each atlas represents a separate Material, and not the Texture Atlas itself. Reusing a texture atlas doesn't affect Draw Calls (outside of the use of another material) and is probably negligible if not non-existent. But using another material increases our worst case. 
## Atlas Problems
### Why Not Merge Atlases Using a Universal Material / Shader
Simply put; because it's easier and prettier. We avoid bleeding problems. Textures that shouldn't be emissive, are not. Textures that shouldn't be transparent, are not. A universal Material would need to prevent these problems at low Mip Levels. Mentioned elsewhere, but this universal texture would need to be processed for the various mip artifacts.
### Do Atlases Need To Be Processed?
Probably the Transparent atlas, but almost definitely for the Emissive atlas. Emissive becomes all black at a specified Mip Level to avoid emission blending. Transparent might need to become completely transparent to avoid weird color blending.
### Why Not Ignore MipMaps ?
Because it looks better. If Unity had mip map levels like their LOD levels, I would ignore planning around MipMaps in a heartbeat. As MipMaps could be limited to one pixel per image in the atlas, and then use that as the minimum level. 
E.G. A 2x2 atlas of 16x16 images would want to stop at 2x2, since that means each image is now a pixel, further mips would merge images.
Since we need to have an all or nothing approach, we need to plan for when the Mips blur together. Hence our use of seperate atlasses for different purposes.
