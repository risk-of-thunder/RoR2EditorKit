using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.PropertyDrawers
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    /// <summary>
    /// A version of the <see cref="ExtendedPropertyDrawer{T}"/> that's used for drawing PropertyDrawers using VisualElements
    /// <para>Automatically retrieves the UXML asset for the property drawer by looking for an UXML asset with the same name as the inheriting type, this UXML will be copied to <see cref="RootVisualElement"/></para>
    /// <para>The VisualElementPropertyDrawer</para> cannot be used for creating IMGUI based UI, for IMGUI UI, use <see cref="IMGUIPropertyDrawer{T}"/>
    /// </summary>
    /// <typeparam name="T">The type used for this property drawer, this can also be of type <see cref="PropertyAttribute"/></typeparam>
    public abstract class VisualElementPropertyDrawer<T> : ExtendedPropertyDrawer<T>
    {
        /// <summary>
        /// The root visual element for this PropertyDrawer
        /// </summary>
        protected VisualElement RootVisualElement
        {
            get
            {
                if (_rootVisualElement == null)
                {
                    _rootVisualElement = new VisualElement();
                    _rootVisualElement.name = "VisualElementPropertyDrawer_RootElement";
                }
                return _rootVisualElement;
            }
        }

        private VisualElement _rootVisualElement;
        /// <summary>
        /// <inheritdoc cref="ExtendedPropertyDrawer{T}.OnGUI(Rect, SerializedProperty, GUIContent)"/>
        /// Sealed as VisualElement property drawers cannot use this method
        /// </summary>
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
        /// <summary>
        /// <inheritdoc cref="ExtendedPropertyDrawer{T}.CreatePropertyGUI(SerializedProperty)"/>
        /// The visual element will be copied to <see cref="RootVisualElement"/>, you can finish the UI using the method <see cref="FinishGUI"/>
        /// </summary>
        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            base.CreatePropertyGUI(property);
            try
            {
                GetTemplateInstance(GetType().Name, RootVisualElement, ValidateUXMLPath);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            FinishGUI();
            return RootVisualElement;
        }
        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making an property drawer that isnt in the same assembly as RoR2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this property drawer, false otherwise</returns>
        protected virtual bool ValidateUXMLPath(string path)
        {
            return VisualElementUtil.ValidateUXMLPath(path);
        }
        /// <summary>
        /// Finish your property drawer's UI here.
        /// </summary>
        protected abstract void FinishGUI();
    }
}
