using System;
using System.Collections;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Entities.UniversalDelegates;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Rendering;
using Random = Unity.Mathematics.Random;

public class InDevVoxelSandboxMaster : MonoBehaviour
{
    public string singletonGameObjectName;

    private GameObject _singleton;
    private string _worldName;
    private VoxelUniverse _universe;

    void Awake()
    {
        //Find Singleton
        _singleton = GameObject.Find(singletonGameObjectName);
        if (_singleton == null)
            throw new NullReferenceException($"Singleton '{singletonGameObjectName}' not found!");

        //Get World Information
        var wi = _singleton.GetComponent<InDevWorldInformation>();
        _worldName = wi.WorldName;

        //Get the world seed
        //Currently uses the name as a hash
        var seed = _worldName.GetHashCode();
        //Create a universe
        _universe = new VoxelUniverse();
        //Create a chunk
        using (var temp = new VoxelChunk(new int3(32)))
        {
            //Get world path
            var fullDir = Path.Combine(InDevPathUtil.WorldDirectory, _worldName);
            var depends = new JobHandle();
            //Initialize to defaults
            depends = new TestJob.FillJob<bool>()
            {
                Value = true,
                Array = temp.Active
            }.Schedule(depends);
            depends = new TestJob.FillJob<byte>()
            {
                Value = 0,
                Array = temp.Identities
            }.Schedule(depends);

            depends.Complete();
            //Save, this is the origin chunk for world 0
            InDevVoxelChunkStreamer.Save(fullDir, 0, new int3(0, 0, 0), temp);
        }
        //Create a chunk
        //Again? OH, For world 1
        using (var temp = new VoxelChunk(new int3(32)))
        {
            //Get world path
            var fullDir = Path.Combine(InDevPathUtil.WorldDirectory, _worldName);
            var depends = new JobHandle();
            //get a random instance
            var rand = new Random((uint) seed);
            //Get random values
            depends = new RandomBoolJob()
            {
                Rand = rand,
                Array = temp.Active
            }.Schedule(depends);
            //Get random IDs
            depends = new RandomByteJob()
            {
                Rand = rand,
                Array = temp.Identities
            }.Schedule(depends);

            depends.Complete();
            InDevVoxelChunkStreamer.Save(fullDir, 1, new int3(0, 0, 0), temp);
        }
    }

    private struct RandomBoolJob : IJob
    {
        public Random Rand;
        public NativeArray<bool> Array;


        public void Execute()
        {
            for (var i = 0; i < Array.Length; i++)
                Array[i] = Rand.NextBool();
        }
    }

    private struct RandomByteJob : IJob
    {
        public Random Rand;
        public NativeArray<byte> Array;


        public void Execute()
        {
            for (var i = 0; i < Array.Length; i++)
                Array[i] = (byte) Rand.NextInt(0, byte.MaxValue + 1);
        }
    }
}

//Collection of Worlds

//Collection Of Chunks