using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using ECS.Voxel.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = System.Random;

public class ChunkData : ISerializable, IDisposable
{
    public NativeBitArray SolidTable; //((2^5)^3)/(2^3) -> (2^2)^3 -> 64 bytes
//32 ^ 3 ->1 32768 bytes


    public NativeArray<Directions> HiddenFaces;
//    public byte[,,] BlockTypeId;


    private const int FlatSize = AxisSize * AxisSize * AxisSize;
    private const int AxisSize = 8;

    public ChunkData()
    {
        SolidTable = new NativeBitArray(FlatSize, Allocator.Persistent);
        HiddenFaces = new NativeArray<Directions>(FlatSize, Allocator.Persistent);
    }

    public ChunkData(SerializationInfo info, StreamingContext context)
    {
        var bytes = info.GetValue<byte[]>("Solidity");
        var dirs = info.GetValue<Directions[]>("Visibility");
        throw new NotImplementedException();
    }

    public const int ChunkSizePerAxis = AxisSize;

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("Version", 0);
        info.AddValue("Solidity", SolidTable);
        info.AddValue("Visibility", HiddenFaces);
    }

    public void Dispose()
    {
        SolidTable.Dispose();
        HiddenFaces.Dispose();
    }
}

public static class RandomCollectionOfExtensions
{
    public static T GetValue<T>(this SerializationInfo info, string name)
    {
        return (T) info.GetValue(name, typeof(T));
    }

    public static byte[] ToBytes(this BitArray bitArray)
    {
        var ret = new byte[(bitArray.Length - 1) / 8 + 1];
        bitArray.CopyTo(ret, 0);
        return ret;
    }

    public static void ToBytes(this BitArray bitArray, ref byte[] array)
    {
//        var ret = new byte[(bitArray.Length - 1) / 8 + 1];
        bitArray.CopyTo(array, 0);
//        return ret;
    }

    public static byte[] ToNativeBytes(this BitArray bitArray, Allocator allocator)
    {
        var ret = new byte[(bitArray.Length - 1) / 8 + 1];
        bitArray.CopyTo(ret, 0);
        return ret;
    }
}

public struct NativeBitArray : IDisposable
{
    private NativeArray<byte> _backing;

    public NativeBitArray(int size, Allocator allocator)
    {
        Count = size;
        var byteCount = size / 8;
        var remainder = size % 8;
        if (remainder > 0)
            byteCount++;

        _backing = new NativeArray<byte>(byteCount, allocator);
    }

    public bool this[int index]
    {
        get
        {
            var flag = 1 << (index % 8);
            return (_backing[index / 8] & flag) == flag;
        }
        set
        {
            var flag = 1 << (index % 8);
            if (value)
                _backing[index / 8] = (byte) (_backing[index / 8] | flag);
            else
                _backing[index / 8] = (byte) (_backing[index / 8] & ~flag);
        }
    }

    public byte GetByte(int index)
    {
        return _backing[index];
    }

    public void SetByte(int index, byte value)
    {
        _backing[index] = value;
    }


    public int Count { get; private set; }
    public int ByteCount => _backing.Length;

    public void Dispose()
    {
        _backing.Dispose();
    }
}

public class ChunkBehaviour : MonoBehaviour
{
    [SerializeField] private GameObject _blockPrefab;
    private ChunkDataEntity _data;


//    const int YOffset =

    private void Start()
    {
        _data = new ChunkDataEntity();
        _data.SpawnEntities(_blockPrefab);
        _data.Init();
    }


    // Update is called once per frame
    void Update()
    {
    }
}

public class ChunkDataEntity
{
    public Entity[] EntityTable;

    public ChunkData Chunk;

    public void Init()
    {
        Init(Chunk);
        UpdateCulled();
    }

    public static void Init(ChunkData data)
    {
        SetupActive(data);
        SetupVisibility(data);
    }

    public static void SetupActive(ChunkData data)
    {
        Random r = new Random();
        foreach (var pos in VoxPos.GetAllPositions())
            data.SolidTable[pos] = true;
    }

    public static void SetupVisibility(ChunkData data)
    {
        foreach (var pos in VoxPos.GetAllPositions())
        {
            var intPos = pos.Position;
            var flags = DirectionsX.NoneFlag;

            if (intPos.x != 0)
                flags |= Directions.Left;
            if (intPos.x != VoxPos.MaxValue)
                flags |= Directions.Right;

            if (intPos.y != 0)
                flags |= Directions.Down;
            if (intPos.y != VoxPos.MaxValue)
                flags |= Directions.Up;

            if (intPos.z != 0)
                flags |= Directions.Backward;
            if (intPos.z != VoxPos.MaxValue)
                flags |= Directions.Forward;

            data.HiddenFaces[pos] = flags;
        }
    }

    public void UpdateCulled()
    {
        foreach (var pos in VoxPos.GetAllPositions())
            UpdateCulled(pos);
    }

    public void UpdateCulled(VoxPos voxPos)
    {
        var em = World.Active.EntityManager;
        var e = EntityTable[voxPos];
        if (Chunk.HiddenFaces[voxPos].IsAll())
        {
            if (!em.HasComponent(e, typeof(Disabled)))
                em.AddComponent(e, typeof(Disabled));
        }
        else
        {
            if (em.HasComponent(e, typeof(Disabled)))
                em.RemoveComponent(e, typeof(Disabled));
        }
    }

    public void SpawnEntities(GameObject prefab, int3 offset = default)
    {
        var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, World.Active);
        var em = World.Active.EntityManager;
        var offsetShift = new float3(1 / 2f);
        foreach (var pos in VoxPos.GetAllPositions())
        {
            var spawned = em.Instantiate(entityPrefab);
            em.SetComponentData(spawned, new Translation {Value = offset + pos.Position + offsetShift});
            EntityTable[pos] = spawned;
        }

        em.DestroyEntity(entityPrefab);
    }

    public ChunkDataEntity() : this(new ChunkData())
    {
    }

    public ChunkDataEntity(ChunkData chunkData)
    {
        EntityTable = new Entity[FlatSize];
        Chunk = chunkData;
    }

    public const int ChunkSizePerAxis = 32;
    public const int FlatSize = ChunkSizePerAxis * ChunkSizePerAxis * ChunkSizePerAxis;
}