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