using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class IMGUIPropertyDrawer<T> : ExtendedPropertyDrawer<T>
    {
        public float standardPropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            DrawIMGUI(position, property, label);
        }

        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return null;
        }

        protected abstract void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return standardPropertyHeight;
        }
        protected float GetWidthForSnugLabel(string label) => GetWidthForSnugLabel(new GUIContent(label));
        protected float GetWidthForSnugLabel(GUIContent label) => EditorStyles.label.CalcSize(label).x;
        protected void DrawPropertyFieldWithSnugLabel(Rect totalPosition, SerializedProperty property, GUIContent label)
        {
            DrawPropertyFieldWithSnugLabel(totalPosition, property, label, true);
        }
        protected void DrawPropertyFieldWithSnugLabel(Rect totalPosition, SerializedProperty property, GUIContent label, bool drawChildren)
        {
            var labelRect = totalPosition;
            labelRect.width = GetWidthForSnugLabel(label) + 20;
            EditorGUI.LabelField(labelRect, label);

            var controlRect = totalPosition;
            controlRect.width -= labelRect.width;
            controlRect.x = labelRect.xMax;
            EditorGUI.PropertyField(controlRect, property, GUIContent.none, drawChildren);
        }
    }
}