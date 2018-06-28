using UnityEditor;
using UnityEngine;
using Voxel;

[CustomPropertyDrawer(typeof(Int3))]
public class Int3Drawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(SerializedPropertyType.Vector3, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var propX = property.FindPropertyRelative("x");
        var propY = property.FindPropertyRelative("y");
        var propZ = property.FindPropertyRelative("z");
        var v = new Vector3(propX.intValue, propY.intValue, propZ.intValue);
        v = EditorGUI.Vector3Field(position, label, v);
        propX.intValue = (int) v.x;
        propY.intValue = (int) v.y;
        propZ.intValue = (int) v.z;
    }
}