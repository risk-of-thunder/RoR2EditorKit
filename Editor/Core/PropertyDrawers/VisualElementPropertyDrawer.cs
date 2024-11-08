using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// An <see cref="ExtendedPropertyDrawer{T}"/> that allows you to implement the UI using VisualElements, it utilizes UXML Templates and finds the proper template by searching for a template that has the same name as your property drawer's class name 
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="ExtendedPropertyDrawer{T}"/></typeparam>
    public abstract class VisualElementPropertyDrawer<T> : ExtendedPropertyDrawer<T>
    {
        /// <summary>
        /// The root visual element for this property drawer
        /// </summary>
        protected VisualElement rootVisualElement
        {
            get
            {
                if (_rootVisualElement == null)
                {
                    _rootVisualElement = new VisualElement();
                    _rootVisualElement.name = $"{typeof(T).Name}_RootElement";
                }
                return _rootVisualElement;
            }
        }
        private VisualElement _rootVisualElement;

        /// <summary>
        /// Not applicable for VisualElement proeprty drawers
        /// </summary>
        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }

        /// <summary>
        /// <inheritdoc cref="ExtendedPropertyDrawer{T}.CreatePropertyGUI(SerializedProperty)"/>
        /// <para>For <see cref="VisualElementPropertyDrawer{T}"/>, utilize <see cref="FinishUI"/> to finalize the template's functionality<</para>
        /// </summary>
        /// <param name="property">The property we're drawing</param>
        /// <returns>The visual element that represents the control for the serialized property</returns>
        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, rootVisualElement, ValidatePath);
            FinishUI();
            return rootVisualElement;
        }


        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making an inspector that isnt in the same assembly as R2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this inspector, false otherwise</returns>
        protected virtual bool ValidatePath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);

        /// <summary>
        /// Utilize this method to initialize the controls for your property drawer, such as ensuring bindings, implementing context menus, etc.
        /// </summary>
        protected abstract void FinishUI();
    }
}