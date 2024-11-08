using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(KinematicCharacterController.ReadOnlyAttribute))]
    public class KCCReadOnlyDrawer : IMGUIPropertyDrawer<KinematicCharacterController.ReadOnlyAttribute>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);
            position.height = GetPropertyHeight(property, label);
            EditorGUI.PropertyField(position, property, true);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : IMGUIPropertyDrawer<ReadOnlyAttribute>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);
            position.height = GetPropertyHeight(property, label);
            EditorGUI.PropertyField(position, property, true);
            EditorGUI.EndDisabledGroup();
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}