﻿using RoR2EditorKit.Settings;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(EditorInspectorSettings.InspectorSetting))]
    public sealed class InspectorSettingDrawer : IMGUIPropertyDrawer
        <EditorInspectorSettings.InspectorSetting>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var isEnabled = property.FindPropertyRelative("isEnabled");
            var displayName = property.FindPropertyRelative("inspectorName");

            GUIContent content = new GUIContent(ObjectNames.NicifyVariableName(displayName.stringValue), "Wether this inspector is enabled");
            EditorGUI.PropertyField(position, isEnabled, content, false);
            EditorGUI.EndProperty();
        }
    }
}
