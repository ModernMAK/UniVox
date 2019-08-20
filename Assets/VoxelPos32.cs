using System.Collections.Generic;
using ECS.Voxel;
using Unity.Mathematics;

public struct VoxelPos32 
{
    public bool Equals(VoxelPos32 other)
    {
        return _backing == other._backing;
    }

    public override bool Equals(object obj)
    {
        return obj is VoxelPos32 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _backing.GetHashCode();
    }

    private const int FullMask = 0x7FFF; // 32^3 - 1
    private const int PartialMask = 0x1F; //31
    private const int BitOffset = 5; //Log2 of 32 = 5

    public const int MaxValue = PartialMask;
    public const int MinValue = 0;
    private readonly short _backing;


    public VoxelPos32(int index)
    {
        _backing = (short) (index & FullMask);
    }

    public VoxelPos32(int3 position)
    {
        _backing = FromXYZ(position.x, position.y, position.z);
    }

    public VoxelPos32(int x, int y, int z)
    {
        _backing = FromXYZ(x, y, z);
    }

    private static short FromXYZ(int x, int y, int z)
    {
        return (short) ((x & PartialMask) | ((y & PartialMask) << BitOffset) | ((z & PartialMask) << (BitOffset * 2)));
    }

    public int x => Get(BitOffset * 0);

    public int y => Get(BitOffset * 1);

    public int z => Get(BitOffset * 2);

    public int3 Position => new int3(x, y, z);

    public int Index => _backing & FullMask;


    public static IEnumerable<VoxelPos32> GetAllPositions()
    {
        for (var i = 0; i <= FullMask; i++)
            yield return new VoxelPos32(i);
    }

    #region Operators

    #region Conversion

    public static implicit operator int3(VoxelPos32 vp)
    {
        return vp.Position;
    }

    public static implicit operator int(VoxelPos32 vp)
    {
        return vp.Index;
    }

    public static explicit operator VoxelPos32(int3 pos)
    {
        return new VoxelPos32(pos);
    }

    public static explicit operator VoxelPos32(int index)
    {
        return new VoxelPos32(index);
    }

    #endregion

    #region Math

    public static VoxelPos32 operator ++(VoxelPos32 vp)
    {
        return new VoxelPos32((vp._backing + 1) & FullMask);
    }


    public static VoxelPos32 operator --(VoxelPos32 vp)
    {
        return new VoxelPos32((vp._backing - 1) & FullMask);
    }


    public static VoxelPos32 operator +(VoxelPos32 left, VoxelPos32 right)
    {
        var size = new int3(PartialMask + 1);
        var result = (left.Position + right.Position) % size;
        return new VoxelPos32(result);
    }

    public static VoxelPos32 operator -(VoxelPos32 left, VoxelPos32 right)
    {
        var size = new int3(PartialMask + 1);
        var result = (left.Position - right.Position + size) % size;
        return new VoxelPos32(result);
    }

    #endregion

    #region Equality

    public static bool operator <(VoxelPos32 left, VoxelPos32 right)
    {
        return left.Index < right.Index;
    }

    public static bool operator >=(VoxelPos32 left, VoxelPos32 right)
    {
        return !(left < right);
    }

    public static bool operator >(VoxelPos32 left, VoxelPos32 right)
    {
        return left.Index > right.Index;
    }

    public static bool operator <=(VoxelPos32 left, VoxelPos32 right)
    {
        return !(left > right);
    }

    public static bool operator ==(VoxelPos32 left, VoxelPos32 right)
    {
        return (left._backing == right._backing);
    }

    public static bool operator !=(VoxelPos32 left, VoxelPos32 right)
    {
        return !(left == right);
    }

    #endregion

    #endregion

    private int Get(int bitOffset)
    {
        return (_backing >> bitOffset) & PartialMask;
    }
}