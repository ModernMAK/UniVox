using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralMesh
{
    public struct DynamicTriangle : IEnumerable<int>
    {
        public DynamicTriangle(int l, int p, int r) : this()
        {
            Pivot = p;
            Left = l;
            Right = r;
        }

        /// <summary>
        /// Copies the dynamic triangle
        /// </summary>
        /// <param name="tri">The triangle to copy. </param>
        public DynamicTriangle(DynamicTriangle tri) : this(tri[0], tri[1], tri[2])
        {
        }

        /// <summary>
        /// Expects triangleArray to have only 3 elements
        /// Ignores elements past 3
        /// Probably will throw index out of range exception before 3, if it doesn't, worry.
        /// </summary>
        /// <param name="triangleArray">The triangle array.</param>
        public DynamicTriangle(IList<int> triangleArray) : this(triangleArray[0], triangleArray[1], triangleArray[2])
        {
        }

        public int Left { get; private set; }
        public int Pivot { get; private set; }
        public int Right { get; private set; }

        public DynamicTriangle SetLeft(int left)
        {
            return new DynamicTriangle(this) {Left = left};
        }
        public DynamicTriangle SetPivot(int pivot)
        {
            return new DynamicTriangle(this) {Pivot = pivot};
        }
        public DynamicTriangle SetRight(int right)
        {
            return new DynamicTriangle(this) {Right = right};
        }
        public DynamicTriangle SetIndex(int index, int value)
        {
            var tri = new DynamicTriangle(this);
            tri[index] = value;
            return tri;
        }

        /// <summary>
        /// Indexable getter/setter
        /// </summary>
        /// <param name="index">The index of the triangle "array".</param>
        /// <exception cref="Exception">Throws an exception if index % 3 does not equal [0,2].</exception>
        public int this[int index]
        {
            get
            {
                switch (index % 3)
                {
                    case 0:
                        return Left;
                    case 1:
                        return Pivot;
                    case 2:
                        return Right;
                    default:
                        throw new Exception("SOMETHING I ASSUMED IMPOSSIBLE HAPPENED!");
                }
            }
            private set
            {
                switch (index % 3)
                {
                    case 0:
                        Left = value;
                        break;
                    case 1:
                        Pivot = value;
                        break;
                    case 2:
                        Right = value;
                        break;
                    default:
                        throw new Exception("SOMETHING I ASSUMED IMPOSSIBLE HAPPENED!");
                }
            }
        }

        /// <summary>
        /// Returns false if the triangle does not form a proper triangle.
        /// In other words, if the triangle has a duplicate vertex, it's invalid.
        /// </summary>
        /// <returns>True if no duplicate verts are found, false otherwise.</returns>
        public bool IsValid()
        {
            for (var i = 0; i < 3; i++)
            for (var j = i + 1; j < 3; j++)
                if (this[i] == this[j])
                    return false;
            return true;
        }

        /// <summary>
        /// Converts any combination of Left Pivot Right, to an ideal combination. 
        /// Triangles with the same verticies will produce the same Ideal Triangle.
        /// </summary>
        /// <returns>An ideal combination of Left Pivot Right.</returns>
        private DynamicTriangle ToIdeal()
        {
            var smallest = this[0];
            var shift = 0;
            for (var i = 1; i < 3; i++)
                if (this[i] < smallest)
                {
                    shift = i;
                    smallest = this[i];
                }

            //No shift? return this, otherwise, return a new triangle shifted
            return shift == 0 ? this : new DynamicTriangle(this[shift + 0], this[shift + 1], this[shift + 2]);
        }

        /// <summary>
        /// Gets a hashcode, uses the ideal triangle to fetch the hashcode.
        /// </summary>
        /// <returns>The Ideal Hashcode.</returns>
        public override int GetHashCode()
        {
            var ideal = ToIdeal();
            return new Vector3(ideal[0], ideal[1], ideal[2]).GetHashCode();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public IEnumerator<int> GetEnumerator()
        {
            for (var i = 0; i < 3; i++)
                yield return this[i];
        }

        /// <summary>
        /// Determines if the obj is a DynamicTriangle and if the obj is equal.
        /// </summary>
        /// <param name="obj">The potential DynamicTriangle.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public override bool Equals(object obj)
        {
            //Not DynamicTriangle? Not Equal.
            if (!(obj is DynamicTriangle)) return false;

            //Convert to ideal
            var other = ((DynamicTriangle) obj).ToIdeal();
            var ideal = ToIdeal();

            //Sweep
            for (var i = 0; i < 3; i++)
                if (ideal[i] != other[i])
                    return false;
            return true;
        }
    }
}