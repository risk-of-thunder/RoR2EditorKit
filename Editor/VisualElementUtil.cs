using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit
{
    /// <summary>
    /// A class containing both extension methods and utility methods
    /// </summary>
    public static class VisualElementUtil
    {

        /// <summary>
        /// Quick method to set the ObjectField's object type
        /// </summary>
        /// <typeparam name="TObj">The type of object to set</typeparam>
        /// <param name="objField">The object field</param>
        public static void SetObjectType<T>(this ObjectField objField) where T : UnityEngine.Object
        {
            objField.objectType = typeof(T);
        }

        /// <summary>
        /// Quick method to Clear a visual element's USS Class List, Hierarchy, and Unbind it from a serializedObject
        /// </summary>
        public static void Wipe(this VisualElement visualElement)
        {
            visualElement.Clear();
            visualElement.ClearClassList();
            visualElement.Unbind();
        }

        /// <summary>
        /// Queries a visual element from the FoldoutElement's container
        /// </summary>
        /// <typeparam name="T">The type of VisualElement to query</typeparam>
        /// <param name="foldout">The foldout to query from</param>
        /// <param name="name">The name of the visual element to query</param>
        /// <param name="className">The class name of the visual element to query</param>
        /// <returns>The queried element if found, null otherwise</returns>
        public static T QContainer<T>(this Foldout foldout, string name = null, string className = null) where T : VisualElement
        {
            return foldout.Q<VisualElement>("unity-content").Q<T>(name, className);
        }

        /// <summary>
        /// Quickly sets the display of a visual element
        /// </summary>
        /// <param name="visualElement">The element to change the display style</param>
        /// <param name="displayStyle">new display style value</param>
        public static void SetDisplay(this VisualElement visualElement, DisplayStyle displayStyle) => visualElement.style.display = displayStyle;

        /// <summary>
        /// Quickly sets the display of a visual elementt
        /// </summary>
        /// <param name="visualElement">The element to change the display style</param>
        /// <param name="display">True if its displayed, false if its hidden</param>
        public static void SetDisplay(this VisualElement visualElement, bool display) => visualElement.style.display = display ? DisplayStyle.Flex : DisplayStyle.None;

        /// <summary>
        /// Normalizes a name for usage on UXML trait attributes
        /// <para>Due to limitations on UIBuilder, the UXML trait's name needs to have a specific name formtting that must match the required property that's going to set the value.</para>
        /// </summary>
        /// <param name="nameofProperty"></param>
        /// <returns>A normalized string for an UXML trait</returns>
        public static string NormalizeNameForUXMLTrait(string nameofProperty) => ObjectNames.NicifyVariableName(nameofProperty).ToLower().Replace(" ", "-");


        /// <summary>
        /// Sets the Top, Right, Bottom and Left border colors to <paramref name="color"/>
        /// </summary>
        /// <param name="color">The color to use</param>
        public static void SetBorderColor(this IStyle style, StyleColor color)
        {
            style.borderLeftColor = color;
            style.borderRightColor = color;
            style.borderTopColor = color;
            style.borderBottomColor = color;
        }

        /// <summary>
        /// Sets the Top, Right, Bottom, and Left border width to <paramref name="width"/>
        /// </summary>
        /// <param name="width">The width of the border</param>
        public static void SetBorderWidth(this IStyle style, StyleFloat width)
        {
            style.borderLeftWidth = width;
            style.borderRightWidth = width;
            style.borderTopWidth = width;
            style.borderBottomWidth = width;
        }
    }
}