using HG;
using System;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(ShowFieldObsoleteAttribute))]
    public class ShowFieldObsoleteAttributeDrawer : DecoratorDrawer
    {
        public override void OnGUI(Rect position)
        {
            Rect extendedPosition = position;
            extendedPosition.yMax += 16;
            extendedPosition.width += 4;
            GUI.Box(extendedPosition, text: null, EditorStyles.helpBox);

            EditorGUI.HelpBox(position, "Obsolete: This field is Obsolete", MessageType.Warning);
        }

        public override float GetHeight()
        {
            return base.GetHeight();
        }
    }

    [CustomPropertyDrawer(typeof(ShowFieldObsoleteVerboseAttribute))]
    public class ShowFieldObsoleteVerboseDrawer : ExtendedPropertyDrawer<ShowFieldObsoleteAttribute>
    {
        public readonly static string defaultMessage = "Obsolete: ";
        public new ShowFieldObsoleteVerboseAttribute attribute => base.attribute as ShowFieldObsoleteVerboseAttribute;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Rect helpBoxRect = position;
            helpBoxRect.xMin += EditorGUI.indentLevel * 10;
            helpBoxRect.height = 18;
            string bestMessage = FindBestMessage();
            GUI.Box(helpBoxRect, new GUIContent(text: null, bestMessage));
            EditorGUI.HelpBox(helpBoxRect, bestMessage, MessageType.Warning);

            using (new EditorGUI.DisabledGroupScope(attribute.isDisabled))
            {
                Rect propertyRect = position;
                propertyRect.yMin = helpBoxRect.yMax;
                EditorGUI.PropertyField(propertyRect, property, label, true);
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label) + 18;
        }

        private string FindBestMessage()
        {
            StringBuilder sb = new StringBuilder(defaultMessage);
            bool appendedMessage = false;
            if (!string.IsNullOrEmpty(attribute.message))
            {
                appendedMessage = true;
                sb.Append(attribute.message);
            }

            var obsoleteAttribute = fieldInfo.GetCustomAttribute<ObsoleteAttribute>();
            if (obsoleteAttribute != null && !string.IsNullOrEmpty(obsoleteAttribute.Message))
            {
                appendedMessage = true;
                sb.Append(obsoleteAttribute.Message);
            }

            var tooltipAttribute = fieldInfo.GetCustomAttribute<TooltipAttribute>();
            if (tooltipAttribute != null && !string.IsNullOrEmpty(tooltipAttribute.tooltip))
            {
                appendedMessage = true;
                sb.Append(tooltipAttribute.tooltip);
            }

            if (!appendedMessage)
            {
                sb.Append("This field is Obsolete.");
            }
            return sb.ToString();
        }
    }
}