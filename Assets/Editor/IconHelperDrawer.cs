using UnityEditor;
using UnityEngine;
using Voxel.Unity;

[CustomPropertyDrawer(typeof(IconHelper))]
public class IconHelperDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(SerializedPropertyType.String, label);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var propName = property.FindPropertyRelative("Name");
        var propIcon = property.FindPropertyRelative("Icon");


        if (label.text != propName.stringValue)
        {
            var posName = position;
            posName.width /= 3f;
            posName.width *= 2f;
            var posIcon = posName;
            posIcon.x += posIcon.width;
            posIcon.width /= 2f;
            EditorGUI.PropertyField(posName, propName, label);
            EditorGUI.PropertyField(posIcon, propIcon, new GUIContent());
        }
        else
        {
            var posName = position;
            posName.width /= 2f;
            var posIcon = posName;
            posIcon.x += posIcon.width;
            EditorGUI.PropertyField(posName, propName, new GUIContent());
            EditorGUI.PropertyField(posIcon, propIcon, new GUIContent());
        }
    }
}