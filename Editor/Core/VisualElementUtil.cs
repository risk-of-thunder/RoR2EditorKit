using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public static class VisualElementUtil
    {
        public static void SetObjectType<T>(this ObjectField objField) where T : UnityEngine.Object
        {
            objField.objectType = typeof(T);
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
    }
}