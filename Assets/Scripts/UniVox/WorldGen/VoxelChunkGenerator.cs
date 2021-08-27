using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UniVox.Utility;

namespace UniVox.WorldGen
{
    public class VoxelChunkGenerator : AbstractGenerator<int3, VoxelChunk>
    {
        public int Seed;
        public float Solidity;
        public int IdentityCount;


        private struct CalculateActive : IJobParallelFor
        {
            public int Seed;
            public IndexConverter3D Converter;
            public float Solidity;
            public float3 Offset;
            public NativeArray<VoxelFlag> Flags;
            public float3 Scale;


            public void Execute(int index)
            {
                var pos = Converter.Expand(index) + Offset;
                pos *= Scale;
                var sample = NoiseWrapper.Perlin.NormalizedSample(new float4(pos, Seed));
                if (sample <= Solidity)
                {
                    Flags[index] |= VoxelFlag.Active;
                }
                else
                    Flags[index] &= ~(VoxelFlag.Active);
            }
        }

        private struct SetActive : IJobParallelFor
        {
            public bool Active;
            public NativeArray<VoxelFlag> Flags;


            public void Execute(int index)
            {
                if (Active)
                {
                    Flags[index] |= VoxelFlag.Active;
                }
                else
                    Flags[index] &= ~(VoxelFlag.Active);
            }
        }

        private struct CalculateRandomIdentity : IJobParallelFor
        {
            public int Seed;
            public IndexConverter3D Converter;
            public float3 Offset;
            public NativeArray<byte> Identities;
            public float3 Scale;


            public void Execute(int index)
            {
                var pos = Converter.Expand(index) + Offset;
                pos *= Scale;
                var sample = NoiseWrapper.Perlin.NormalizedSample(new float4(pos, Seed));
                Identities[index] = (byte)(int)math.lerp(byte.MinValue, byte.MaxValue, sample);
            }
        }
        private struct SetIdentities : IJobParallelFor
        {
            public byte Identity;
            public NativeArray<byte> Identities;


            public void Execute(int index)
            {
                Identities[index] = Identity;
            }
        }
        private struct SetPerlinIdentities : IJobParallelFor
        {
            public int Seed;
            public IndexConverter3D Converter;
            public float3 Offset;
            public NativeArray<byte> Identities;
            public byte Identity;
            public float Chance;
            public float3 Scale;


            public void Execute(int index)
            {
                var pos = Converter.Expand(index) + Offset;
                pos *= Scale;
                var sample = NoiseWrapper.Perlin.NormalizedSample(new float4(pos, Seed));
                if (sample <= Chance)
                    Identities[index] = Identity;
            }
        }
        private struct FullGen : IJobParallelFor
        {
            public int Seed;
            public IndexConverter3D Converter;
            public float3 Offset;
            public NativeArray<byte> Identities;
            public NativeArray<VoxelFlag> Flags;
            public byte Identity;
            public float Chance;
            public float3 Scale;


            public void Execute(int index)
            {
                var pos = Converter.Expand(index) + Offset;
                pos *= Scale;
                var sample = NoiseWrapper.Perlin.NormalizedSample(new float4(pos, Seed));
                if (sample <= Chance)
                {
                    Identities[index] = Identity;
                    Flags[index] |= VoxelFlag.Active;
                }
            }
        }

        public override JobHandle Generate(int3 key, VoxelChunk value, JobHandle depends)
        {
            var converter = new IndexConverter3D(value.ChunkSize);
            var invSize = 1f / (float3)value.ChunkSize;
            const int innerBatchSize = byte.MaxValue;
            depends = new CalculateActive()
            {
                Flags = value.Flags,
                Converter = converter,
                Offset = key,
                Scale = invSize,
                Seed = Seed,
                Solidity = Solidity
            }.Schedule(value.Flags.Length, innerBatchSize, depends);

            if (IdentityCount == 0)
            {
                depends = new CalculateRandomIdentity()
                {
                    Identities = value.Identities,
                    Converter = converter,
                    Offset = key,
                    Scale = invSize,
                    Seed = Seed,
                }.Schedule(value.Flags.Length, innerBatchSize, depends);
            }  
            else
            {
                depends = new SetIdentities()
                {
                    Identities = value.Identities,
                    Identity = 0
                }.Schedule(value.Identities.Length, innerBatchSize, depends);

                for (var i = 1; i < IdentityCount; i++)
                {
                    depends = new SetPerlinIdentities()
                    {
                        Identity = (byte)i,
                        Identities = value.Identities,
                        Converter = converter,
                        Offset = key,
                        Scale = invSize,
                        Seed = Seed + i,
                        Chance = 1f / IdentityCount // Just to make sure theres some variety
                    }
                    .Schedule(value.Identities.Length, innerBatchSize, depends);
                }
            }
               
            return depends;
        }
    }
}