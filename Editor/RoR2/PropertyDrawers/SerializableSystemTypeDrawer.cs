using HG;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableSystemType))]
    internal sealed class SerializableSystemTypePropertyDrawer : IMGUIPropertyDrawer<SerializableSystemType>
    {
        private struct NewPropertyValueContainer
        {
            public SerializedProperty targetProperty;
            public string newStringValue;
        }

        private NewPropertyValueContainer? newPropertyValueContainer;

        private static bool _useFullName = false;

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!IsFullyQualified(property))
            {
                FullyQualify(property);
            }

            SerializedProperty assemblyQualifiedNameProperty = property.FindPropertyRelative("assemblyQualifiedName");

            var _drawer = new SerializableSystemTypeDrawer(GetBaseType());
            _drawer.useFullName = _useFullName;

            _drawer.onTypeSelectedHandler += (item) =>
            {
                newPropertyValueContainer = new NewPropertyValueContainer
                {
                    targetProperty = assemblyQualifiedNameProperty,
                    newStringValue = item.assemblyQualifiedName
                };
            };
            _drawer.DoEditorGUI(position, property.FindPropertyRelative("assemblyQualifiedName").stringValue, label);

            if(newPropertyValueContainer.HasValue && SerializedProperty.EqualContents(newPropertyValueContainer.Value.targetProperty, assemblyQualifiedNameProperty))
            {
                assemblyQualifiedNameProperty.stringValue = newPropertyValueContainer.Value.newStringValue;
                serializedObject.ApplyModifiedProperties();
                newPropertyValueContainer = null;
            }
        }

        private Type GetBaseType()
        {
            var attribute = fieldInfo.GetCustomAttribute<SerializableSystemType.RequiredBaseTypeAttribute>();
            if (attribute != null)
            {
                return attribute.type;
            }
            return null;
        }

        private bool IsFullyQualified(SerializedProperty property)
        {
            var p = property.FindPropertyRelative("assemblyQualifiedName");
            var typeName = p.stringValue;
            return Type.GetType(typeName) != null;
        }

        private void FullyQualify(SerializedProperty property)
        {
            var p = property.FindPropertyRelative("assemblyQualifiedName");
            var typeName = p.stringValue;
            var t = Type.GetType($"{typeName}, RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            if (t != null) //Type found, fully qualify it.
            {
                p.stringValue = t.AssemblyQualifiedName;
                p.serializedObject.ApplyModifiedProperties();
            }
        }

        public SerializableSystemTypePropertyDrawer()
        {
            _useFullName = propertyDrawerPreferenceSettings.GetOrCreateSetting("useFullNameForItems", false);
        }
    }

    public class SerializableSystemTypeDrawer
    {
        public bool useFullName;
        public Type requiredBaseType { get; private set; }
        public event Action<InheritingTypeSelectDropdown.Item> onTypeSelectedHandler;

        public void DoEditorGUI(Rect position, string assemblyQualifiedName, GUIContent label)
        {
            var typeName = Type.GetType(assemblyQualifiedName)?.Name ?? "No Type Set";

            var prefixRect = EditorGUI.PrefixLabel(position, label);

            if (EditorGUI.DropdownButton(prefixRect, new GUIContent(typeName, assemblyQualifiedName), FocusType.Passive))
            {
                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), requiredBaseType, useFullName);
                dropdown.onItemSelected += (item) =>
                {
                    onTypeSelectedHandler?.Invoke(item);
                };
                dropdown.Show(position);
            }
        }

        public void DoIMGUILayout(string assemblyQualifiedName, GUIContent label)
        {
            var typeName = Type.GetType(assemblyQualifiedName)?.Name ?? "No State Type";

            var rect = EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            if (EditorGUILayout.DropdownButton(new GUIContent(typeName, assemblyQualifiedName), FocusType.Passive))
            {
                Rect labelRect = GUILayoutUtility.GetLastRect();

                var rectToUse = new Rect
                {
                    x = rect.x + labelRect.width,
                    y = rect.y,
                    height = rect.height,
                    width = rect.width - labelRect.width
                };

                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), requiredBaseType, useFullName);
                dropdown.onItemSelected += (item) => onTypeSelectedHandler?.Invoke(item);
                dropdown.Show(rectToUse);
            }
            EditorGUILayout.EndHorizontal();
        }

        public VisualElement DoVisualElement(string assemblyQualifiedName, GUIContent label)
        {
            return new IMGUIContainer(() =>
            {
                DoIMGUILayout(assemblyQualifiedName, label);
            });
        }

        public SerializableSystemTypeDrawer(Type requiredBaseType)
        {
            this.requiredBaseType = requiredBaseType;
        }
    }
}