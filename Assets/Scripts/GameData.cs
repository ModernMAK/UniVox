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
    }

    private readonly IRegistry<string,int,Mesh> _meshes;
    private readonly IRegistry<string,int,Material> _materials;
    private readonly IRegistry<string,int,Sprite> _sprites;
    private readonly IRegistry<string,int,Texture> _textures;
    private readonly IRegistry<string, int, BlockInfo> _blocks;
    
    
    public IRegistry<string, int, Mesh> Meshes => _meshes;
    public IRegistry<string, int, Material> Materials => _materials;
    public IRegistry<string, int, Sprite> Sprites => _sprites;
    public IRegistry<string, int, Texture> Textures => _textures;
    public IRegistry<string, int, BlockInfo> Blocks => _blocks;
    
    
}

public struct BlockInfo
{
    public BlockInfo(int matId, int iconId)
    {
        Material = matId;
        Icon = iconId;
    }
    
    public int Material { get; }
    public int Icon { get; }
    
}