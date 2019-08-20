using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
struct SetupActiveJob : IJobParallelFor
{
    [WriteOnly] public NativeBitArray Solidity;

    public void Execute(int index)
    {
//        for (var i = 0; i < 8; i++)
//            SolidityWrite[index * 8 + i] = true;
        Solidity.SetByte(index, byte.MaxValue);
    }
}