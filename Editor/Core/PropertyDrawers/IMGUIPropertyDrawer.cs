using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.PropertyDrawers
{
    /// <summary>
    /// A version of the <see cref="ExtendedPropertyDrawer{T}"/> that's used mainly for drawing PropertyDrawers using IMGUI
    /// <para>The IMGUIPropertyDrawer cannot be used for creating VisualElement based UI, for VisualElement UI, use <see cref="VisualElementPropertyDrawer{T}"/></para>
    /// </summary>
    /// <typeparam name="T">The type used for this property drawer, this can also be of type <see cref="PropertyAttribute"/></typeparam>
    public abstract class IMGUIPropertyDrawer<T> : ExtendedPropertyDrawer<T>
    {
        /// <summary>
        /// Returns the addition of <see cref="EditorGUIUtility.singleLineHeight"/> and <see cref="EditorGUIUtility.standardVerticalSpacing"/>
        /// </summary>
        public float StandardPropertyHeight => EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        /// <summary>
        /// Implement your IMGUI property drawer here, <see cref="EditorGUI.BeginProperty(Rect, GUIContent, SerializedProperty)"/> and <see cref="EditorGUI.EndProperty"/> are not called.
        /// </summary>
        /// <param name="position">The position used for this property drawer</param>
        /// <param name="property">The property that's being drawn</param>
        /// <param name="label">The label for this property drawer</param>
        protected abstract void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label);
        /// <summary>
        /// <inheritdoc cref="ExtendedPropertyDrawer{T}.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        /// Sealed for security reasons, implement your IMGUI property drawer on <see cref="DrawIMGUI(Rect, SerializedProperty, GUIContent)"/>
        /// </summary>
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            DrawIMGUI(position, property, label);
        }
        /// <summary>
        /// <inheritdoc cref="ExtendedPropertyDrawer{T}.CreatePropertyGUI(SerializedProperty)"/>
        /// Sealed as an IMGUIPropertyDrawer cannot work with VisualElements.
        /// </summary>
        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }

        /// <summary>
        /// Returns the default property height for this SerializedProperty, defaults to <see cref="EditorGUIUtility.singleLineHeight"/>
        /// <para>It's recommended that you use <see cref="EditorGUI.GetPropertyHeight(SerializedProperty)"/> and it's overloads for getting the height of properties.</para>
        /// </summary>
        /// <param name="property">The SerializedProperty that's being drawn</param>
        /// <param name="label">The label for this property</param>
        /// <returns>The height used for this property</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
