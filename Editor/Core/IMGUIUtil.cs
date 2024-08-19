using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    public static class IMGUIUtil
    {
        public static void PropertyFieldWithSnugLabel(SerializedProperty property, bool includeChildren = true, float extraWidth = 20)
        {
            var origLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(property.displayName, property.tooltip)).x + extraWidth;
            EditorGUILayout.PropertyField(property, includeChildren);
            EditorGUIUtility.labelWidth = origLabelWidth;
        }
    }
}