using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UniVox.Managers;

//[CreateAssetMenu(menuName = "Custom Assets/AtlasMaterial",fileName= "AtlasMaterial")]
//[Serializable]
//public class AtlasAsset : ScriptableObject
//{
//    public Material Material;
//
//    public NamedValue<Rect>[] Rectangles;
//}


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
