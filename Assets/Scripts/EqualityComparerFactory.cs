using System;
using System.Collections.Generic;

    public static class EqualityComparerFactory
    {
        public static IEqualityComparer<T> CreateComparer<T>(Func<T, int> getHashCodeFunc, Func<T, T, bool> equalsFunc)
        {
            if (getHashCodeFunc == null)
                throw new ArgumentNullException("getHashCodeFunc");
            if (equalsFunc == null)
                throw new ArgumentNullException("equalsFunc");

            return new MyComparer<T>(getHashCodeFunc, equalsFunc);
        }

        //FROM https://stackoverflow.com/questions/3189861/pass-a-lambda-expression-in-place-of-icomparer-or-iequalitycomparer-or-any-singl
        private class MyComparer<T> : IEqualityComparer<T>
        {
            private readonly Func<T, T, bool> _equalsFunc;
            private readonly Func<T, int> _getHashCodeFunc;

            public MyComparer(Func<T, int> getHashCodeFunc, Func<T, T, bool> equalsFunc)
            {
                _getHashCodeFunc = getHashCodeFunc;
                _equalsFunc = equalsFunc;
            }

            public bool Equals(T x, T y)
            {
                return _equalsFunc(x, y);
            }

            public int GetHashCode(T obj)
            {
                return _getHashCodeFunc(obj);
            }
        }
    }
