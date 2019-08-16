using System.Collections.Generic;
using ECS.Voxel;
using Unity.Mathematics;

public struct VoxPos
{
    private const int FullMask = 0x7FFF;
    private const int PartialMask = 0x1F; //31
    private const int BitOffset = 5; //Log2 of 32 = 5

    public const int MaxValue = PartialMask;
    public const int MinValue = 0;
    private readonly short _backing;


    public VoxPos(int index)
    {
        _backing = (short) (index & FullMask);
    }

    public VoxPos(int3 position)
    {
        _backing = FromXYZ(position.x, position.y, position.z);
    }

    public VoxPos(int x, int y, int z)
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


    public static IEnumerable<VoxPos> GetAllPositions()
    {
        for (var i = 0; i <= FullMask; i++)
            yield return new VoxPos(i);
    }

    #region Operators

    #region Conversion

    public static implicit operator int3(VoxPos vp)
    {
        return vp.Position;
    }

    public static implicit operator int(VoxPos vp)
    {
        return vp.Index;
    }

    public static explicit operator VoxPos(int3 pos)
    {
        return new VoxPos(pos);
    }

    public static explicit operator VoxPos(int index)
    {
        return new VoxPos(index);
    }

    #endregion

    #region Math

    public static VoxPos operator ++(VoxPos vp)
    {
        return new VoxPos((vp._backing + 1) & FullMask);
    }


    public static VoxPos operator --(VoxPos vp)
    {
        return new VoxPos((vp._backing - 1) & FullMask);
    }


    public static VoxPos operator +(VoxPos left, VoxPos right)
    {
        var size = new int3(PartialMask + 1);
        var result = (left.Position + right.Position) % size;
        return new VoxPos(result);
    }

    public static VoxPos operator -(VoxPos left, VoxPos right)
    {
        var size = new int3(PartialMask + 1);
        var result = (left.Position - right.Position + size) % size;
        return new VoxPos(result);
    }

    #endregion

    #region Equality

    public static bool operator <(VoxPos left, VoxPos right)
    {
        return left.Index < right.Index;
    }

    public static bool operator >=(VoxPos left, VoxPos right)
    {
        return !(left < right);
    }

    public static bool operator >(VoxPos left, VoxPos right)
    {
        return left.Index > right.Index;
    }

    public static bool operator <=(VoxPos left, VoxPos right)
    {
        return !(left > right);
    }

    public static bool operator ==(VoxPos left, VoxPos right)
    {
        return (left._backing == right._backing);
    }

    public static bool operator !=(VoxPos left, VoxPos right)
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