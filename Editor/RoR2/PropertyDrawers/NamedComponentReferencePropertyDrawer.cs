using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    public abstract class NamedComponentReferencePropertyDrawer<T> : IMGUIPropertyDrawer<T> where T : UnityEngine.Component
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            label.text += " | " + (property.objectReferenceValue ? GetName(property.objectReferenceValue as T) : "Null");
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndProperty();
        }

        protected abstract string GetName(T propertyObjectReference);
    }
}