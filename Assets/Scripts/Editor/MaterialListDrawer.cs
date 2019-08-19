//using UnityEditor;
//using UnityEngine;
//
//[CustomPropertyDrawer(typeof(MaterialList))]
//public class MaterialListDrawer : PropertyDrawer
//{
//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        var realProperty = property.FindPropertyRelative("_backingList");
//        EditorGUI.PropertyField(position, realProperty, label);
//    }
//
//    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//    {
//        var realProperty = property.FindPropertyRelative("_backingList");
//        return base.GetPropertyHeight(realProperty, label);
//    }
//}    