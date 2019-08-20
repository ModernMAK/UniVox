using System.Collections.Generic;
using ECS.Voxel.Data;
using UnityEngine;

public class VoxelRenderBehaviour : MonoBehaviour
{
    public MaterialList _Materials;

    public MeshList _Meshes;
    private IDictionary<BlockShape, Mesh> _MeshesDict;
    private MeshFilter _mf;
    private MeshRenderer _mr;
    public bool Hidden;
    public int MaterialIndex;
    public BlockShape Shape;


    // Start is called before the first frame update
    private void Start()
    {
        _mf = GetComponent<MeshFilter>();
        _mr = GetComponent<MeshRenderer>();
        _MeshesDict = _Meshes.CreateDictionary();
    }

    // Update is called once per frame
    private void Update()
    {
        _mr.enabled = !Hidden;
        _mr.material = _Materials[(MaterialIndex % _Materials.Count + _Materials.Count) % _Materials.Count];
        _mf.mesh = _MeshesDict[Shape];
    }
}

//
//public interface IIndexed<in TIndex, TData>
//{
//    TData this[TIndex index] { get; set; }
//}
//
////
//public struct ChunkIndex
//{
//    public ChunkIndex(int index)
//    {
//        _backing = (ushort) index;
//    }
//
//    public ChunkIndex(byte x, byte y, byte z)
//    {
//        _backing = (x % DimensionSize)
//    }
//
//    public const byte DimensionSize = 32;
//    private short _backing;
//
//    public int X
//    {
//        get => _backing % DimensionSize;
//        set
//        {
//            _backing = _backing & ~0b11111; 
//            
//        }
//    }
//
//    public int Y => (_backing >> 5) % DimensionSize;
//    public int Z => (_backing >> 10) % DimensionSize;
//    public int3 Position => new int3(X, Y, Z);
//    public int Index => _backing;
//
//    public static ChunkIndex operator ++(ChunkIndex chunkIndex)
//    {
//        return new ChunkIndex();
//    }
//}
//
//public class Chunk<TData> : IIndexed<ChunkIndex, TData>, IIndexed<int3, TData>
//{
//    public TData this[ChunkIndex index]
//    {
//        get => this[index.Position];
//        set => this[index.Position] = value;
//    }
//
//    public TData this[int3 index]
//    {
//        get => this[index.x, index.y, index.z];
//        set => this[index.Pos] = value;
//    }
//}