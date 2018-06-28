//using UnityEngine;
//
//namespace Voxel
//{
//    public struct Rot3
//    {
//        //Constructors
//        public Rot3(int x, int y = 0, int z = 0) : this()
//        {
//            this.x = x;
//            this.y = y;
//            this.z = z;
//        }
//
////        public Rot3(Vector3 v) : this(new Int3(v))
////        {
////        }
//
//        public Rot3(Int3 v) : this(v.x, v.y, v.z)
//        {
//        }
//
//        public Rot3(Quaternion v) : this(new Int3(v.eulerAngles))
//        {
//        }
//
//        private Int3 _rotation;
//
//        public int x
//        {
//            get { return _rotation.x; }
//            set { _rotation.x = Wrap(value); }
//        }
//
//        public int y
//        {
//            get { return _rotation.y; }
//            set { _rotation.y = Wrap(value); }
//        }
//
//        public int z
//        {
//            get { return _rotation.z; }
//            set { _rotation.z = Wrap(value); }
//        }
//
//        public static readonly Rot3 Identity = new Rot3(0, 0, 0);
//
//        public static Rot3 FromToRotation(VoxelDirection from, VoxelDirection to)
//        {
//            return FromToRotation(from.ToVector(), to.ToVector());
//        }
//
//        public static Rot3 FromToRotation(Int3 from, Int3 to)
//        {
//            return new Rot3(Quaternion.FromToRotation((Vector3) from, (Vector3) to));
//        }
//
//        private static int Wrap(int v)
//        {
//            return Mathf.RoundToInt(((v + 360) % 360) / 90f) * 90;
//        }
//
//        //Conversion Operators
//        public static explicit operator Int3(Rot3 v)
//        {
//            return new Int3(v._rotation);
//        }
//
//        public static explicit operator Rot3(Int3 v)
//        {
//            return new Rot3(v);
//        }
//
//        public static explicit operator Quaternion(Rot3 v)
//        {
//            return Quaternion.Euler(v.x, v.y, v.z);
//        }
//
//        public static explicit operator Rot3(Quaternion v)
//        {
//            return new Rot3(v);
//        }
//
//        //Addition
//        public static Rot3 operator +(Rot3 l)
//        {
//            return l;
//        }
//
//        public static Rot3 operator +(Rot3 l, Rot3 r)
//        {
//            return new Rot3((Quaternion) l * (Quaternion) r);
//        }
//
//        //Subtraction
//        public static Rot3 operator -(Rot3 l)
//        {
//            return new Rot3(l.x + 180, l.y + 180, l.z + 180);
//        }
//
//        public static Rot3 operator -(Rot3 l, Rot3 r)
//        {
//            return (l + -r);
//        }
//
//        //Multiplication
//        public static Int3 operator *(Rot3 l, Int3 r)
//        {
//            var v = ((Quaternion) l * (Vector3) r);
//            return new Int3(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));
//        }
//
//        public static Rot3 operator *(Rot3 l, Rot3 r)
//        {
//            return l + r;
//        }
//
//        public static Rot3 operator *(Rot3 l, int r)
//        {
//            return new Rot3(l.x * r, l.y * r, l.z * r);
//        }
//
//        public static Rot3 operator *(int l, Rot3 r)
//        {
//            return r * l;
//        }
//
//        //Division
//        public static Rot3 operator /(int l, Rot3 r)
//        {
//            return new Rot3(l / r.x, l / r.y, l / r.z);
//        }
//
//        public static Rot3 operator /(Rot3 l, int r)
//        {
//            return new Rot3(l.x / r, l.y / r, l.z / r);
//        }
//
//        //Equality
//        public static bool operator ==(Rot3 l, Rot3 r)
//        {
//            return (Quaternion) l == (Quaternion) r;
//        }
//
//        public static bool operator !=(Rot3 l, Rot3 r)
//        {
//            return !(l == r);
//        }
//
//        //Equality Functions       
//        public bool Equals(Rot3 other)
//        {
//            return this == other;
//        }
//
//        public override bool Equals(object obj)
//        {
//            if (ReferenceEquals(null, obj)) return false;
//            return obj is Int3 && Equals((Int3) obj);
//        }
//
//        public override int GetHashCode()
//        {
//            unchecked
//            {
//                var hashCode = x;
//                hashCode = (hashCode * 397) ^ y;
//                hashCode = (hashCode * 397) ^ z;
//                return hashCode;
//            }
//        }
//    }
//}