using System.Collections;
using System.Runtime.Serialization;
using Unity.Collections;

public static class RandomCollectionOfExtensions
{
    public static T GetValue<T>(this SerializationInfo info, string name)
    {
        return (T) info.GetValue(name, typeof(T));
    }

    public static byte[] ToBytes(this BitArray bitArray)
    {
        var ret = new byte[(bitArray.Length - 1) / 8 + 1];
        bitArray.CopyTo(ret, 0);
        return ret;
    }

    public static void ToBytes(this BitArray bitArray, ref byte[] array)
    {
//        var ret = new byte[(bitArray.Length - 1) / 8 + 1];
        bitArray.CopyTo(array, 0);
//        return ret;
    }

    public static byte[] ToNativeBytes(this BitArray bitArray, Allocator allocator)
    {
        var ret = new byte[(bitArray.Length - 1) / 8 + 1];
        bitArray.CopyTo(ret, 0);
        return ret;
    }
}