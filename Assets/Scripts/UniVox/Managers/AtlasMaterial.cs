using System;
using UnityEngine;
using UniVox.Types;

namespace UniVox
{
    [Serializable]
    public class NamedRect : NamedValue<Rect>
    {
    }

    [Serializable]
    public struct AtlasMaterial
    {
        public Material Material;

        public NamedRect[] Rectangles;
    }
}