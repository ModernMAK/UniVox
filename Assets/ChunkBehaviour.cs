using UnityEngine;

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
    private void Update()
    {
    }
}