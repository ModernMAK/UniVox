# Mechanics
## Recipes / Crafting
No matter what, i like the idea of multiple crafting stations. This unfortunately creates some problems.
Recipes vs Patterns. Patterns are engaging, but multiple crafting stations with similiar patterns can be confusing. This does give us some leeway with the crafting grid, since it can use slots.
Recipes are simple, and if recipes are used, they need to be more engaging than a name in a list. Recipes make autocrafting simple (since we just create a queue of recipes.)

Something we should avoid is restricting recipes to 'tiers' a-la a tier 1 machine cant but a tier 2 can, if they are seperate machines, then they should be named differently, and hopefully act differently. 

Perhaps, allow the player to use any crafting bench nearby. 'Terraria already does that'



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
			- Gunsmith : Guns
			- Blacksmith : Weapons
			- Arcsmith : Magical Things
			- Armorsmiths : Armor
			- Tailor : Clothes
			- Chemist : Healing
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
	 	- Rogues 
		- Wizards
		- Fighters
		- Medics
	* Guards
* Skilled
 	- Doctor : Heals
	- Scientists : Performs Research

# Mechanic Implementations
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
