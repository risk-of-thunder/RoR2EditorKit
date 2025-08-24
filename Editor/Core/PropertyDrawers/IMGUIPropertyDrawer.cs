using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// A <see cref="ExtendedPropertyDrawer{T}"/> that allows you to implement the UI using IMGUI, instead of directly interfacing with VisualElements
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="ExtendedPropertyDrawer{T}"/></typeparam>
    public abstract class IMGUIPropertyDrawer<T> : ExtendedPropertyDrawer<T>
    {
        /// <summary>
        /// Returns the sum of <see cref="EditorGUIUtility.singleLineHeight"/> and <see cref="EditorGUIUtility.standardVerticalSpacing"/>
        /// </summary>
        public float standardPropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        /// <summary>
        /// <inheritdoc cref="ExtendedPropertyDrawer{T}.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        /// <para>For <see cref="IMGUIPropertyDrawer{T}"/>, utilize <see cref="DrawIMGUI(Rect, SerializedProperty, GUIContent)"/> instead to create your IMGUI Property</para>
        /// </summary>
        /// <param name="position">The total position for the property drawer</param>
        /// <param name="property">The property drawer we're drawing</param>
        /// <param name="label">The label for the property drawer</param>
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            DrawIMGUI(position, property, label);
        }

        /// <summary>
        /// Not applicable for IMGUI Property Drawers
        /// </summary>
        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return null;
        }

        /// <summary>
        /// Implement your IMGUI Property drawer using this method
        /// </summary>
        /// <param name="position">The total position for the property drawer</param>
        /// <param name="property">The property drawer we're drawing</param>
        /// <param name="label">The label for the property drawer</param>
        protected abstract void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            base.GetPropertyHeight(property, label);
            return standardPropertyHeight;
        }

        /// <summary>
        /// Returns the minimum width for the label provided, useful for creating controls with snug labels
        /// </summary>
        /// <param name="label">The label to calculate it's width</param>
        /// <returns>The minmum width for this label</returns>
        protected float GetWidthForSnugLabel(string label) => GetWidthForSnugLabel(new GUIContent(label));

        /// <summary>
        /// Returns the minimum width for the label provided, useful for creating controls with snug labels
        /// </summary>
        /// <param name="label">The label to calculate it's width</param>
        /// <returns>The minmum width for this label</returns>
        protected float GetWidthForSnugLabel(GUIContent label) => EditorStyles.largeLabel.CalcSize(label).x + 10;

        /// <summary>
        /// Draws a PropertyField with a snug label
        /// </summary>
        /// <param name="totalPosition">The total position for the control</param>
        /// <param name="property">The property to draw</param>
        /// <param name="label">The field's label</param>
        protected void DrawPropertyFieldWithSnugLabel(Rect totalPosition, SerializedProperty property, GUIContent label)
        {
            DrawPropertyFieldWithSnugLabel(totalPosition, property, label, true);
        }

        /// <summary>
        /// Draws a PropertyField with a snug label
        /// </summary>
        /// <param name="totalPosition">The total position for the control</param>
        /// <param name="property">The property to draw</param>
        /// <param name="label">The field's label</param>
        /// <param name="drawChildren">Wether children should be drawn</param>
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