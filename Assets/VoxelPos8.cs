using System.Collections.Generic;
using Unity.Mathematics;

public struct VoxelPos8 
{
    public bool Equals(VoxelPos8 other)
    {
        return _backing == other._backing;
    }

    public override bool Equals(object obj)
    {
        return obj is VoxelPos8 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _backing.GetHashCode();
    }

    private const int FullMask = 0x1FF;//511
    private const int PartialMask = 0x07; //7
    private const int BitOffset = 3; //Log2 of 8 = 3

    public const int MaxValue = PartialMask;
    public const int MinValue = 0;
    private readonly short _backing;


    public VoxelPos8(int index)
    {
        _backing = (short) (index & FullMask);
    }

    public VoxelPos8(int3 position)
    {
        _backing = FromComponents(position.x, position.y, position.z);
    }

    public VoxelPos8(int x, int y, int z)
    {
        _backing = FromComponents(x, y, z);
    }

    private static short FromComponents(int x, int y, int z)
    {
        return (short) ((x & PartialMask) | ((y & PartialMask) << BitOffset) | ((z & PartialMask) << (BitOffset * 2)));
    }

    public int x => Get(BitOffset * 0);

    public int y => Get(BitOffset * 1);

    public int z => Get(BitOffset * 2);

    public int3 Position => new int3(x, y, z);

    public int Index => _backing & FullMask;


    public static IEnumerable<VoxelPos8> GetAllPositions()
    {
        for (var i = MinValue; i <= MaxValue; i++)
            yield return new VoxelPos8(i);
    }

    #region Operators

    #region Conversion

    public static implicit operator int3(VoxelPos8 vp)
    {
        return vp.Position;
    }

    public static implicit operator int(VoxelPos8 vp)
    {
        return vp.Index;
    }

    public static explicit operator VoxelPos8(int3 pos)
    {
        return new VoxelPos8(pos);
    }

    public static explicit operator VoxelPos8(int index)
    {
        return new VoxelPos8(index);
    }

    #endregion

    #region Math

    public static VoxelPos8 operator ++(VoxelPos8 vp)
    {
        return new VoxelPos8((vp._backing + 1) & FullMask);
    }


    public static VoxelPos8 operator --(VoxelPos8 vp)
    {
        return new VoxelPos8((vp._backing - 1) & FullMask);
    }


    public static VoxelPos8 operator +(VoxelPos8 left, VoxelPos8 right)
    {
        var size = new int3(PartialMask + 1);
        var result = (left.Position + right.Position) % size;
        return new VoxelPos8(result);
    }

    public static VoxelPos32 operator -(VoxelPos8 left, VoxelPos8 right)
    {
        var size = new int3(PartialMask + 1);
        var result = (left.Position - right.Position + size) % size;
        return new VoxelPos32(result);
    }

    #endregion

    #region Equality

    public static bool operator <(VoxelPos8 left, VoxelPos8 right)
    {
        return left.Index < right.Index;
    }

    public static bool operator >=(VoxelPos8 left, VoxelPos8 right)
    {
        return !(left < right);
    }

    public static bool operator >(VoxelPos8 left, VoxelPos8 right)
    {
        return left.Index > right.Index;
    }

    public static bool operator <=(VoxelPos8 left, VoxelPos8 right)
    {
        return !(left > right);
    }

    public static bool operator ==(VoxelPos8 left, VoxelPos8 right)
    {
        return (left._backing == right._backing);
    }

    public static bool operator !=(VoxelPos8 left, VoxelPos8 right)
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