using System;
using System.Collections.Generic;
using StandardAssets.Characters.Effects;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace StandardAssets.Characters.Editor
{
    /// <summary>
    /// Custom property drawer visualizing <see cref="MovementEventZoneDefinitionList"/> as a reorderable list
    /// </summary>
    [CustomPropertyDrawer(typeof(MovementEventZoneDefinitionList))]
    public class MovementEventZoneDefinitionListPropertyDrawer : PropertyDrawer
    {
        //Property names used in FindPropertyRelative calls
        const string k_ListPropertyName = "m_MovementZoneLibraries";
        const string k_PhysicMaterialPropertyName= "m_PhysicMaterial";
        const string k_ZoneLibraryPropertyName= "m_ZoneLibrary";
        const string k_LeftFootPropertyName = "m_LeftFootEffects";
        const string k_RightFootPropertyName = "m_RightFootEffects";
        const string k_LandingPropertyName = "m_LandingEffects";
        const string k_JumpingPropertyName = "m_JumpingEffects";
        
        //Header text on the ReorderableList GUI
        const string k_Header = "Movement Zones";
        
        //Cached ReorderableList object
        ReorderableList m_ReorderableList;

        //Reference to root property, i.e. MovementEventZoneDefinitionList
        SerializedProperty m_RootProperty;
        
        //Reference to list property, i.e. m_MovementZoneLibraries
        SerializedProperty m_ListProperty;
        
        //List of heights of different elements of the reorderable list
        List<float> m_ElementHeights = new List<float>();

        // Sets the ReorderableList, its events and all required support variables 
        void SetupReorderableList(SerializedProperty property)
        {
            if (m_ReorderableList != null)
            {
                return;
            }

            m_RootProperty = property;

            m_ListProperty = property.FindPropertyRelative(k_ListPropertyName);
            for (int i = 0; i < m_ListProperty.arraySize; i++)
            {
                m_ElementHeights.Add(2 * EditorGUIUtility.singleLineHeight);
            }

            m_ReorderableList = new ReorderableList(m_ListProperty.serializedObject, m_ListProperty, false, false, true, true);
            m_ReorderableList.drawHeaderCallback = rect =>
            {
                GUI.Label(rect, k_Header);
            };

            m_ReorderableList.drawElementCallback = DrawElementCallback;
            m_ReorderableList.elementHeightCallback = ElementHeightCallback;
            m_ReorderableList.onAddCallback = OnAddCallback;
            m_ReorderableList.onRemoveCallback = OnRemoveCallback;
            m_ReorderableList.onChangedCallback = OnChangedCallback; 
        }

        /// <summary>
        /// Change callback used to apply the properties
        /// </summary>
        /// <param name="list">ReorderableList parameter required by the ReorderableList</param>
        void OnChangedCallback(ReorderableList list)
        {
            m_RootProperty.serializedObject.ApplyModifiedProperties();
        }

        // Remove callback used to decrease the size of the element height list
        void OnRemoveCallback(ReorderableList list)
        {
            m_ElementHeights.RemoveAt(list.index);

            list.serializedProperty.DeleteArrayElementAtIndex(list.index);
            if (list.index >= list.serializedProperty.arraySize - 1)
            {
                list.index = list.serializedProperty.arraySize - 1;
            }
        }

        // Add callback used to increase the size of the element height list
        void OnAddCallback(ReorderableList list)
        {
            m_ElementHeights.Add(2 * EditorGUIUtility.singleLineHeight);
            m_ListProperty.arraySize++;
        }

        // Callback used to adjust the rendered height of an element
        float ElementHeightCallback(int index)
        {
            return m_ElementHeights[index];
        }

        // Wrapper to make the drawing of a single effect property easier
        float DrawEffectProperty(string propertyName, SerializedProperty parent, ref Rect rect)
        {
            var property = parent.FindPropertyRelative(propertyName);
            var height = EditorGUI.GetPropertyHeight(property, true);
            EditorGUI.PropertyField(rect, property, true);
            rect.y += height;
            return height;
        }

        // Callback for drawing and element
        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var elementHeight = EditorGUIUtility.singleLineHeight;
            var elementProperty = m_ListProperty.GetArrayElementAtIndex(index);

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(rect, elementProperty.FindPropertyRelative(k_PhysicMaterialPropertyName), true);
            rect.y += EditorGUIUtility.singleLineHeight;
            elementHeight += EditorGUIUtility.singleLineHeight;

            //don't draw the default serialized class but instead just draw the internal properties so that it reads better in the Inspector
            var zoneLibary = elementProperty.FindPropertyRelative(k_ZoneLibraryPropertyName);
            EditorGUI.indentLevel++;
            elementHeight += DrawEffectProperty(k_LeftFootPropertyName, zoneLibary, ref rect);
            elementHeight += DrawEffectProperty(k_RightFootPropertyName, zoneLibary, ref rect);
            elementHeight += DrawEffectProperty(k_LandingPropertyName, zoneLibary, ref rect);
            elementHeight += DrawEffectProperty(k_JumpingPropertyName, zoneLibary, ref rect);
            EditorGUI.indentLevel--;

            m_ElementHeights[index] = elementHeight;
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(m_RootProperty.serializedObject.targetObject);
            }
        }

        /// <summary>
        /// Overrides the rendered height of the property drawer
        /// </summary>
        /// <param name="property">Reference to SerializedProperty in question</param>
        /// <param name="label">Reference to GUIContent</param>
        /// <returns>Rendered height of the property drawer</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return m_ReorderableList == null ? base.GetPropertyHeight(property, label) : m_ReorderableList.GetHeight();
        }

        /// <summary>
        /// Overrides the default rendering behaviour of the SerializedProperty
        /// </summary>
        /// <param name="position">Position of the property</param>
        /// <param name="property">SerializedProperty being rendered</param>
        /// <param name="label">Reference to GUIContent</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SetupReorderableList(property);
            m_ReorderableList.DoList(position);
            m_RootProperty.serializedObject.ApplyModifiedProperties();
        }
    }
}
