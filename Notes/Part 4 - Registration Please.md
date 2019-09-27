# A Big Mistake
Registration is painful and clunky. It's as simple as that, I'm going to try my best to justify this garbage design decision in one sentence. ECS can't use refernce types in Jobs. You'd think that I could work around this, but that still requires assigning arbitrary numbers to value types, which is exactly what a registry does.

So, let's list the pains, and find ways to work around them.
PROBLEMS
1) The Inheritance Tree is a mess.
2) RegistryReferences are a hassle to work with
3) Should Types Cache References or Data? (Or even cache at all?)
4) We Use Wierd Types to access nested data (EG BlockIdentity, coupled strongly to the registry, but registry knows nothing about it.
5) System types are impossibly difficult to get (EG debug/error mat).
6) Prototyping is painful

SOLUTIONS
1) Okay, solving problem 1. We only use one kind of registry (the auto registry) and even then, we use a subclass called NamedRegistry. Lets stick to a single registry solution for now.
2) Maybe have Registry Reference implicitly cast to Index or Value type. Names should only ever be used for looking up the the true ID.
3) A Reference should be equivalent to the data, (as per solution 2), they should probably cache it. Leaving the cache primarily for systems to access when needed.
4) Since we simplify the Inheritence Heiarchy, perhaps we could complicate things with nested types? Kinda a copout against simplicity.
5) Seperate Fallback 'Registry'. Can't register new things once created.
6) Use scriptable objects? The biggest part of prototyping is Blocks, making creating those easy makes most everything else easier.

---
Alright, lets try this out 
---
# Thoughts 