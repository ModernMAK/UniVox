using System;
using UnityEngine;

namespace Types.Native
{
    public interface INativeMesh : IDisposable
    {
        void FillInto(Mesh m);
    }
}