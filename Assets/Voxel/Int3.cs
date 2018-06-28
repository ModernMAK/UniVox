using System.Collections.Generic;
using UnityEngine;

namespace Voxel
{
    [System.Serializable]
    public struct Int3
    {
        //Constructors
        public Int3(int x, int y = 0, int z = 0) : this()
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

//        public Int3(Vector3 v) : this((int) v.x, (int) v.y, (int) v.z)
//        {
//        }

        public Int3(Int3 v) : this(v.x, v.y, v.z)
        {
        }

        //Properties
        public int x;

        public int y;
        public int z;

        //Global instances
        public static readonly Int3 One = new Int3(1, 1, 1);

        public static readonly Int3 Right = new Int3(1);
        public static readonly Int3 Left = -Right;
        public static readonly Int3 Up = new Int3(0, 1);
        public static readonly Int3 Down = -Up;
        public static readonly Int3 Forward = new Int3(0, 0, 1);
        public static readonly Int3 Back = -Forward;
        public static readonly Int3 Zero = new Int3(0);

        //Static Funcs
        public static Int3 Scale(Int3 l, Int3 r)
        {
            return new Int3(l.x * r.x, l.y * r.y, l.z * r.z);
        }

        //Static Funcs
        public static Int3 InvScale(Int3 l, Int3 r)
        {
            return new Int3(l.x / r.x, l.y / r.y, l.z / r.z);
        }

        //Conversion Operators
        public static explicit operator Vector3(Int3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }


        public static Int3 Floor(Vector3 v)
        {
            return new Int3(Mathf.FloorToInt(v.x), Mathf.FloorToInt(v.y), Mathf.FloorToInt(v.z));
        }
        
        public static Int3 Ceil(Vector3 v)
        {
            return new Int3(Mathf.CeilToInt(v.x), Mathf.CeilToInt(v.y), Mathf.CeilToInt(v.z));
        }
        
        public static Int3 Round(Vector3 v)
        {
            return new Int3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
        }

//        public static explicit operator Int3(Vector3 v)
//        {
//            return new Int3(v);
//        }

        //Addition
        public static Int3 operator +(Int3 l)
        {
            return l;
        }

        public static Int3 operator +(Int3 l, Int3 r)
        {
            return new Int3(l.x + r.x, l.y + r.y, l.z + r.z);
        }

        //Iterates over the entire volume, traversing X's first, Y's second, and Z's last
        //IE For Unit Zero to Unit One (0,0,0) -> (1,0,0) -> (0,1,0) -> (1,1,0) -> (0,0,1) -> (1,0,1) -> (1,1,1)

        public static IEnumerable<Int3> RangeEnumerable(Int3 to, bool inclusive = false)
        {
            return RangeEnumerable(Zero, to, inclusive);
        }

        public static IEnumerable<Int3> RangeEnumerable(Int3 from, Int3 to, bool inclusive = false)
        {
            for (var x = from.x; (inclusive ? (x <= to.x) : (x < to.x)); x++)
            for (var y = from.y; (inclusive ? (y <= to.y) : (y < to.y)); y++)
            for (var z = from.z; (inclusive ? (z <= to.z) : (z < to.z)); z++)
            {
                yield return new Int3(x, y, z);
            }
        }

        //Subtraction
        public static Int3 operator -(Int3 l)
        {
            return new Int3(-l.x, -l.y, -l.z);
        }

        public static Int3 operator -(Int3 l, Int3 r)
        {
            return new Int3(l.x - r.x, l.y - r.y, l.z - r.z);
        }

        //Multiplication
        public static Int3 operator *(Int3 l, int r)
        {
            return new Int3(l.x * r, l.y * r, l.z * r);
        }

        public static Int3 operator *(int l, Int3 r)
        {
            return r * l;
        }

        //Division
        public static Int3 operator /(int l, Int3 r)
        {
            return new Int3(l / r.x, l / r.y, l / r.z);
        }

        public static Int3 operator /(Int3 l, int r)
        {
            return new Int3(l.x / r, l.y / r, l.z / r);
        }

        //Equality
        public static bool operator ==(Int3 l, Int3 r)
        {
            return (l.x == r.x && l.y == r.y && l.z == r.z);
        }

        public static bool operator !=(Int3 l, Int3 r)
        {
            return !(l == r);
        }

        //Equality Functions       
        public bool Equals(Int3 other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Int3 && Equals((Int3) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x;
                hashCode = (hashCode * 397) ^ y;
                hashCode = (hashCode * 397) ^ z;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
    }
}