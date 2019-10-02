//using UnityEditor;
//using UnityEngine;
//
//[CustomPropertyDrawer(typeof(MaterialList))]
//public class MaterialListDrawer : PropertyDrawer
//{
//    public override void OnGUI(Rect Position, SerializedProperty property, GUIContent label)
//    {
//        var realProperty = property.FindPropertyRelative("_backingList");
//        EditorGUI.PropertyField(Position, realProperty, label);
//    }
//
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        var realProperty = property.FindPropertyRelative("_backingList");
//        return base.GetPropertyHeight(realProperty, label);
//    }
//}    