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
    internal sealed class SerializableSystemTypePropertyDrawer : ExtendedPropertyDrawer<SerializableSystemType>
    {
        private struct NewPropertyValueContainer
        {
            public SerializedProperty targetProperty;
            public string newStringValue;
        }

        private NewPropertyValueContainer? newPropertyValueContainer;

        private static bool _useFullName = false;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            EnsureFullyQualified(property);

            var advancedDropdownElement = new AdvancedDropdownElement(property.GetGUIContent(), GetCurrentValue(property), (element) =>
            {
                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), GetBaseType(), _useFullName);
                dropdown.onItemSelected += element.OnItemSelected;
                return dropdown;
            }, GetDropdownButtonLabel);
            advancedDropdownElement.RegisterValueChangedCallback(evt =>
            {
                var item = (InheritingTypeSelectDropdown.Item)evt.newValue;

                string typeName = "";
                if (item.isValid)
                {
                    typeName = item.assemblyQualifiedName;
                }

                property.FindPropertyRelative("assemblyQualifiedName").stringValue = typeName;
                property.serializedObject.ApplyModifiedProperties();
            });
            advancedDropdownElement.showMixedValue = property.hasMultipleDifferentValues;

            return advancedDropdownElement;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            EnsureFullyQualified(property);

            SerializedProperty assemblyQualifiedNameProperty = property.FindPropertyRelative("assemblyQualifiedName");

            using(new EditorGUI.PropertyScope(position, label, property))
            {
                Rect controlRect = EditorGUI.PrefixLabel(position, label);
                if(EditorGUI.DropdownButton(controlRect, GetDropdownButtonLabel(GetCurrentValue(property)), FocusType.Passive))
                {
                    var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), GetBaseType(), _useFullName);
                    dropdown.onItemSelected += (item) =>
                    {
                        newPropertyValueContainer = new NewPropertyValueContainer
                        {
                            newStringValue = item.assemblyQualifiedName,
                            targetProperty = assemblyQualifiedNameProperty
                        };
                    };
                    dropdown.Show(position);
                }

                if (newPropertyValueContainer.HasValue && SerializedProperty.EqualContents(newPropertyValueContainer.Value.targetProperty, assemblyQualifiedNameProperty))
                {
                    assemblyQualifiedNameProperty.stringValue = newPropertyValueContainer.Value.newStringValue;
                    serializedObject.ApplyModifiedProperties();
                    newPropertyValueContainer = null;
                }
            }
        }

        private InheritingTypeSelectDropdown.Item GetCurrentValue(SerializedProperty property)
        {
            SerializedProperty assemblyQualifiedNameProperty = property.FindPropertyRelative("assemblyQualifiedName");

            if (string.IsNullOrWhiteSpace(assemblyQualifiedNameProperty.stringValue))
                return null;

            Type type = Type.GetType(assemblyQualifiedNameProperty.stringValue);
            if (type == null)
                return null;

            return new InheritingTypeSelectDropdown.Item(_useFullName ? type.FullName : type.Name, type.Name, type.FullName, type.AssemblyQualifiedName);
        }

        private GUIContent GetDropdownButtonLabel(AdvancedDropdownItem value)
        {
            if(value is not InheritingTypeSelectDropdown.Item item || !item.isValid)
            {
                return new GUIContent($"No {GetBaseType()?.Name ?? "Type"} Set");
            }

            string label = item.typeName;
            string tooltip = item.assemblyQualifiedName;

            return new GUIContent(label, tooltip);
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

        private void EnsureFullyQualified(SerializedProperty property)
        {
            if (IsFullyQualified(property) == false)
            {
                FullyQualify(property);
            }
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

    [Obsolete("The SerializableEntityStateTypeDrawer is Obsolete")]
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