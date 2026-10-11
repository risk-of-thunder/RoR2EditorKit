using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ChildLocator.NameTransformPair))]
    public sealed class ChildLocatorNameTransformPairPropertyDrawer : IMGUIPropertyDrawer<ChildLocator.NameTransformPair>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using(new EditorGUI.PropertyScope(position, label, property))
            {
                var nameRect = position;
                nameRect.width /= 2;
                nameRect.height = EditorGUIUtility.singleLineHeight;

                var transformRect = nameRect;
                transformRect.x = nameRect.xMax;

                DrawPropertyFieldWithSnugLabel(nameRect, property.FindPropertyRelative(nameof(ChildLocator.NameTransformPair.name)), new GUIContent("Name"));
                DrawPropertyFieldWithSnugLabel(transformRect, property.FindPropertyRelative(nameof(ChildLocator.NameTransformPair.transform)), new GUIContent("Transform"));
            }
        }
    }
}