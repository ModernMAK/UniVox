using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniVox.Managers;

public class GameData
{
    private static GameData _gameData;

    public static GameData Instance
    {
        get
        {
            if (_gameData == null)
            {
                _gameData = new GameData();
            }

            return _gameData;
        }
    }

    private GameData()
    {
        _meshes = new SimpleRegistry<Mesh>();
        _materials = new SimpleRegistry<Material>();
        _sprites= new SimpleRegistry<Sprite>();
        _textures = new SimpleRegistry<Texture>();
        _blocks = new SimpleRegistry<BlockInfo>();
        _items = new SimpleRegistry<IItem>();
    }

    private readonly IRegistry<string,int,Mesh> _meshes;
    private readonly IRegistry<string,int,Material> _materials;
    private readonly IRegistry<string,int,Sprite> _sprites;
    private readonly IRegistry<string,int,Texture> _textures;
    private readonly IRegistry<string, int, BlockInfo> _blocks;
    private readonly IRegistry<string, int, IItem> _items;
    
    
    public IRegistry<string, int, Mesh> Meshes => _meshes;
    public IRegistry<string, int, Material> Materials => _materials;
    public IRegistry<string, int, Sprite> Sprites => _sprites;
    public IRegistry<string, int, Texture> Textures => _textures;
    public IRegistry<string, int, BlockInfo> Blocks => _blocks;
    public IRegistry<string, int, IItem> Items => _items;

    public NativeArray<int> GetBlockToMaterial(Allocator allocator = Allocator.Persistent)
    {
        var hashmap = new NativeArray<int>(_blocks.Count,allocator);
        for (var i = 0; i < _blocks.Count; i++)
        {
            hashmap[i] = _blocks[i].Material;
        }
        return hashmap;
    }
}


public struct BlockInfo
{
    public BlockInfo(int matId)
    {
        Material = matId;
    }
    
    public int Material { get; }
}