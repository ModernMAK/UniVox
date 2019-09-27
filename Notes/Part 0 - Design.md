# Vocabulary
**Block**: A 'Block' is a Voxel's render information
**Voxel**: A 'Voxel' is just a point with information.
**Chunk**: A 'Chunk' is a collections of Voxels. Chunks have a predefined size
**World**: A 'World' is a collection of Chunks.
**Universe**: A 'Universe' is a collection of Worlds.
# Design 
A Voxel represents the core of the game. For all intents and purposes, a Block will store the Rendering Information.

A Block needs to know its worldspace position, & rotation, its 'Shape', 'Material' & whether it has been culled. *Therefore* a Block knows it's **Position**, **Rotation**, **Mesh**, **Material**, and whether it has been **Culled**.




# Mechanics
## Wires & Pipes
Stolen from Minecraft's many 'Tech' Mods. Variants should include; Power Wires, Item, Liquid, Gas, Maybe more?
## Jobs & Towns & Economy & Citizens
### Towns 
Towns can be constructed. Not sure how yet. Towns need Townies
### Economy
Towns have an economy, based on the jobs filled, and the Town inventory. If an item is low and useful, they will pay more, if an item is plentiful or useless, they will pay less. This will also help traders adjust their prices.
### Jobs
There are many Jobs for townies. Lets see how specific we can get.
* Crafters
	* Items
		* Carpenter?
	* Equipment
		* Smiths - (Guns/Armor/Sword/Shield)
* Traders
	* Shopkeeper
	* Wandering Trader
* Food Supply
	* Producer
		* Farmer
		* Rancher
	* Gatherer
		* Hunters
		* Foragers
	* Processor
		* Butcher
		* Chef
		* Baker?
* Laborers
	* Lumberjacks
	* Miners
* Militia
	* Adventurers (Mercenaries)
	* Guards

# Mechanic Implimentations
## Jobs
Jobs should probably require a Workspace, maybe a specific room, or a specific object.
Jobs should be simple, they should do ONE THING. If they dont do one thing, then Jobs should be composed of CHORES, a CHORE is one thing that a JOB can do. This keeps things simple, and allows us to reduce the complexity of a JOB.
By making CHORES a single action, we can allow Villagers to have Complex Jobs which overlap.

Examples of CHORES for some Jobs
* Guards
	* Find a Hostile Creature
	* Patrol
	* Respond To Distress (Alarm / Injury)
	* Attack Hostiles
* Miners
	* Find Ore
	* Mine Ore
	* Replace Unstable / Broken Blocks
	
Alternatively Jobs could act as State Machines, I will call these BEHAVIOURS

* Guards
	* Patrol
		* Move To Point
		* Wait
		* Decide Behaviour
	* Protect

The main difference i See is that BEHAVIOURS dictate their future BEHAVIOURS (and thus know state), while CHORES do not. The downside i see to behaviours is that transitions would need to be defined, where as chores could be chosen by using a blackboard of information, and weighting it against Chores, then selecting the highest. (Find L4D Speach AI GDC for an example)


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

 
