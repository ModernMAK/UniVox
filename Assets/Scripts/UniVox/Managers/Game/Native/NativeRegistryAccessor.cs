using System;
using Unity.Collections;
using UniVox.Launcher;
using UniVox.Managers.Game.Accessor;

namespace UniVox.Managers.Game.Native
{
    public static class NativeRegistryAccessorUtil
    {
        public static NativeHashMap<TIdentity, TValue> CreateNative<TKey, TIdentity, TValue>(
            this RegistryWrapper<TKey, TIdentity, TValue> registry)
            where TIdentity : struct, IEquatable<TIdentity> where TValue : struct
        {
            //Lets assume we only need a byte (which is highly unlikely)
            var map = new NativeHashMap<TIdentity, TValue>(byte.MaxValue, Allocator.Persistent);
            foreach (var pair in registry.GetAllRegistered()) map[pair.Identity] = pair.Value;

            return map;
        }

        public static NativeHashMap<TIdentity, NativeBaseBlockReference> CreateNativeBlockMap<TKey, TIdentity>(
            this RegistryWrapper<TKey, TIdentity, BaseBlockReference> registry)
            where TIdentity : struct, IEquatable<TIdentity>

        {
            //Lets assume we only need a byte (which is highly unlikely)
            var map = new NativeHashMap<TIdentity, NativeBaseBlockReference>(byte.MaxValue, Allocator.Persistent);
            foreach (var pair in registry.GetAllRegistered()) map[pair.Identity] = pair.Value.GetNative();

            return map;
        }
    }
}