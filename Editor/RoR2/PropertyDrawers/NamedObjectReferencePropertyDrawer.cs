using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    public abstract class NamedObjectReferencePropertyDrawer<T> : IMGUIPropertyDrawer<T> where T: UnityEngine.Component
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect contentRect = EditorGUI.PrefixLabel(position, label);

            Rect labelRect = contentRect;
            labelRect.width *= 0.3f;

            if (property.objectReferenceValue != null)
            {
                GUIContent labelLabel = new GUIContent(GetName(property.objectReferenceValue as T));
                EditorGUI.LabelField(labelRect, labelLabel);
            }

            Rect objectFieldRect = labelRect;
            objectFieldRect.x = labelRect.xMax;
            objectFieldRect.width = contentRect.width - labelRect.width;

            Object currentObject = EditorGUI.ObjectField(objectFieldRect, property.objectReferenceValue, typeof(T), true);

            property.objectReferenceValue = currentObject;

            EditorGUI.EndProperty();
        }

        protected abstract string GetName(T propertyObjectReference);
    }
}