using RoR2EditorKit.PropertyDrawers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(KinematicCharacterController.ReadOnlyAttribute))]
    public class KCCReadOnlyDrawer : IMGUIPropertyDrawer<KinematicCharacterController.ReadOnlyAttribute>
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndDisabledGroup();
        }
    }
}
