//    using DungeonGen;
//using UnityEditor;
//using UnityEngine;
////
////[CustomPropertyDrawer(typeof(Int2))]
////public class Int2Drawer : PropertyDrawer
////{
////    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
////    {
////        return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector2, label);
////    }
////
////    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
////    {
////        var propX = property.FindPropertyRelative("x");
////        var propY = property.FindPropertyRelative("y");
////        var v = new Vector2(propX.intValue, propY.intValue);
////        v = EditorGUI.Vector2Field(position, label, v);
////        propX.intValue = (int) (v.x);
////        propY.intValue = (int) (v.y);
////    }
////}