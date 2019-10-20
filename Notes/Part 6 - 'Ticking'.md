I was thinking about how the registry currently treats blocks. And how i would impliment 'Ticking' when it occured to me.

What blocks need ticks? Thinking about vanilla minecraft, most blocks dont tick. Blocks that do tick would probably be better as entities in my implimentation (really all that comes to mind are saplings and and crops, both are not 'blocks' in the conventional sense)

So if most blocks are decorative, should we make non-decorative blocks entities or make decorative blocks 'special'

Before i get ahead of myself, I was thinking about 'BlockIds', and how they are currently 5 bytes, one per mod and 4 for blocks. Personally, this should probably shrink to 4 bytes (1.5 mod, 2.5 blocks), but that got me thinking. Do we need ModIds?

Currently ModKeys help segment names that would be similiar. But ModIds dont help with that, as 'Iron A' and 'Iron B' can have drastically different BlockIds. ModIds (thinking from Fallout) makes it easy to determine what something is from, and lets mod authors control IDS, but do we need to worry about controlling IDs? While it's beneficial to be able to say 'type additem 0019 to get iron' you could just as easily say 'type additem univox~iron' which is clunkier but easier to remember. Mixing (00~iron or univox~19) is an extra layer of work which further obfuscates the problem (and adds some confusing syntax to the console system). 

So, we drop mod ids. This just gives us the one int to work with, yay! But, ints are big; do we need an int? Probably not, (and to be honest, we probably never needed an int) a short will work just fine. (I'm pretty sure classic only had maybe 64 blocks, and i think modern vanilla hasn't even broken 255. These are greate arguments for a byte, so why a short? Well 255 is a hard cieling to break with one game, but modded, that cieling will break easily).

So we have brought 5 bytes to 2 bytes. This still leaves us with the problem; do we need 2 bytes? Blocks that are decorative only differ in their appaearence; which would primarily be 'Transparency', 'Color', 'Shape', (Culling Method?), and the 'Texture/SubTex (Really, a material)

That could be described as a flag (1bit), a simplified pallete (8bit), an enum (atmost 8bit), and the texture (?bit/?bit atmost both 32).
We'd need 81 bits or about 11 bytes to capture all of this information. Some of these can probably be captures in block metadata, like color and shape. that brings us down to 9 bytes (Tex/SubTex + Transparency), preferabbly transparency can be calculated.
That brings us down to 8 bytes. Assuming every block only needs one texture (a pretty safe assumption) we get down to 6 bytes...

And because i completely spaced, subtex is actually 6 times it's size! 32+2 bytes gives us 34!
But most textures cant support (2^32) subtextures (that'd be REAL BIG). Lets assume at most they'd store at max 255. That's 8 bytes! Pretty good, but we can do better. Chances are subtextures only have 16 textures. (The max im using is 3 right now, and i could see reason for reach 6 & 12, 16 gives us some leeway with that, and fits nicely in 4 bits) That gives us 4 bytes for a block's texture...


Does this help? A Decorative block would need to store 4 bytes + metadata (2 bytes atm)...


I think the answer to this problem is; blocks should just natively be decorative. If it needs to be ticked, chances are its not a 'block' but an object in the world.

Examples off the top of my head are plants, machines, pipes, all these aren't exactly blocks. They are still placed in the world, and whatnot.

I'll probably work on 'block composition later'