using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class EntityTester : MonoBehaviour
{
}

//public class BlockSystem : 

public static class CreationEngine
{
    public static TestUniverse CreateUniverse() => new TestUniverse();


    public static bool TryCreateWorld(this TestUniverse testUniverse, int index, out TestWorld world,
        string name = default)
    {
        if (!testUniverse.ContainsKey(index))
        {
            world = testUniverse[index] = new TestWorld(name);
            return true;
        }

        world = default;
        return false;
    }

    public static bool TryCreateChunk(this TestWorld testWorld, int3 index, out TestChunk world,
        Allocator allocator = Allocator.Persistent, NativeArrayOptions options = NativeArrayOptions.ClearMemory)
    {
        if (!testWorld.ContainsKey(index))
        {
            world = testWorld[index] = new TestChunk(allocator, options);
            return true;
        }

        world = default;
        return false;
    }

    public static void FillChunk(this TestChunk chunk, Entity entity, EntityManager manager = default)
    {
        if (manager == default)
            manager = World.Active.EntityManager;

        manager.Instantiate(entity, chunk.Handle);
    }
    
    
}


public class TestUniverse : IReadOnlyDictionary<int, TestWorld>
{
    public TestUniverse()
    {
        _backing = new Dictionary<int, TestWorld>();
    }

    private readonly Dictionary<int, TestWorld> _backing;

    public IEnumerator<KeyValuePair<int, TestWorld>> GetEnumerator()
    {
        return _backing.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _backing.Count;

    public bool ContainsKey(int key)
    {
        return _backing.ContainsKey(key);
    }

    public bool TryGetValue(int key, out TestWorld value)
    {
        return _backing.TryGetValue(key, out value);
    }

    public TestWorld this[int key]
    {
        get => _backing[key];
        set => _backing[key] = value;
    }

    public IEnumerable<int> Keys => _backing.Keys;

    public IEnumerable<TestWorld> Values => _backing.Values;
}

public class TestWorld : IReadOnlyDictionary<int3, TestChunk>
{
    public TestWorld(string name = default)
    {
        _backing = new Dictionary<int3, TestChunk>();
        _entityWorld = new World(name);
    }

    private readonly World _entityWorld;
    private readonly Dictionary<int3, TestChunk> _backing;

    public World EntityWorld => _entityWorld;
    public EntityManager EntityManager => _entityWorld.EntityManager;

    public IEnumerator<KeyValuePair<int3, TestChunk>> GetEnumerator()
    {
        return _backing.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => _backing.Count;

    public bool ContainsKey(int3 key)
    {
        return _backing.ContainsKey(key);
    }

    public bool TryGetValue(int3 key, out TestChunk value)
    {
        return _backing.TryGetValue(key, out value);
    }

    public TestChunk this[int3 key]
    {
        get => _backing[key];
        set => _backing[key] = value;
    }

    public IEnumerable<int3> Keys => _backing.Keys;

    public IEnumerable<TestChunk> Values => _backing.Values;
}

public class TestChunk : IReadOnlyDictionary<int3, Entity>
{
    public TestChunk(Allocator allocator = Allocator.Persistent,
        NativeArrayOptions options = NativeArrayOptions.ClearMemory)
    {
        _backing = new NativeArray<Entity>(CubeSize, allocator, options);
    }

    private static IReadOnlyList<int3> InternalKeysCached;

    private static IReadOnlyList<int3> InternalKeys
    {
        get
        {
            if (InternalKeysCached != null) return InternalKeysCached;


            var temp = new int3[CubeSize];
            for (var x = 0; x < AxisSize; x++)
            for (var y = 0; y < AxisSize; y++)
            for (var z = 0; z < AxisSize; z++)
            {
                var i = x + y * AxisSize + z * SquareSize;
                temp[i] = new int3(x, y, z);
            }

            InternalKeysCached = temp;

            return InternalKeysCached;
        }
    }


    public const byte AxisSize = 8;
    public const short SquareSize = AxisSize * AxisSize;
    public const int CubeSize = SquareSize * AxisSize;

    private readonly NativeArray<Entity> _backing;

    public NativeArray<Entity> Handle => _backing;

    public IEnumerator<KeyValuePair<int3, Entity>> GetEnumerator()
    {
        for (var i = 0; i < CubeSize; i++)
        {
            var pos = InternalKeys[i];
            var entity = _backing[i];
            yield return new KeyValuePair<int3, Entity>(pos, entity);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => CubeSize;

    public bool ContainsKey(int3 key)
    {
        return (key.x >= 0 && key.x <= AxisSize) &&
               (key.y >= 0 && key.y <= AxisSize) &&
               (key.z >= 0 && key.z <= AxisSize);
    }

    public bool TryGetValue(int3 key, out Entity value)
    {
        if (ContainsKey(key))
        {
            value = this[key];
            return true;
        }

        value = default;
        return false;
    }

    public Entity this[int3 key] => _backing[key.x + key.y * AxisSize + key.z * SquareSize];

    public IEnumerable<int3> Keys => InternalKeys;

    public IEnumerable<Entity> Values => _backing;
}