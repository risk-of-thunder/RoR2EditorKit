using EntityStates;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(SerializableEntityStateType))]
    internal sealed class SerializableEntityStateTypePropertyDrawer : ExtendedPropertyDrawer<SerializableEntityStateType>
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
                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), typeof(EntityState), _useFullName);
                dropdown.onItemSelected += element.OnItemSelected;
                return dropdown;
            }, GetDropdownButtonLabel);
            advancedDropdownElement.RegisterValueChangedCallback(evt =>
            {
                var item = (InheritingTypeSelectDropdown.Item)evt.newValue;

                string typeName = "";
                if(item.isValid)
                {
                    typeName = item.assemblyQualifiedName;
                }

                property.FindPropertyRelative("_typeName").stringValue = typeName;
                property.serializedObject.ApplyModifiedProperties();
            });
            advancedDropdownElement.showMixedValue = property.hasMultipleDifferentValues;

            return advancedDropdownElement;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            EnsureFullyQualified(property);

            SerializedProperty _typeNameProperty = property.FindPropertyRelative("_typeName");

            using(new EditorGUI.PropertyScope(position, label, property))
            {
                Rect controlRect = EditorGUI.PrefixLabel(position, label);
                if(EditorGUI.DropdownButton(controlRect, GetDropdownButtonLabel(GetCurrentValue(property)), FocusType.Passive))
                {
                    var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), typeof(EntityState), _useFullName);
                    dropdown.onItemSelected += (item) =>
                    {
                        newPropertyValueContainer = new NewPropertyValueContainer
                        {
                            newStringValue = item.assemblyQualifiedName,
                            targetProperty = _typeNameProperty
                        };
                    };
                    dropdown.Show(position);
                }

                if (newPropertyValueContainer.HasValue && SerializedProperty.EqualContents(newPropertyValueContainer.Value.targetProperty, _typeNameProperty))
                {
                    _typeNameProperty.stringValue = newPropertyValueContainer.Value.newStringValue;
                    serializedObject.ApplyModifiedProperties();
                    newPropertyValueContainer = null;
                }
            }
        }

        private InheritingTypeSelectDropdown.Item GetCurrentValue(SerializedProperty property)
        {
            SerializedProperty _typeNameProperty = property.FindPropertyRelative("_typeName");

            if (string.IsNullOrWhiteSpace(_typeNameProperty.stringValue))
                return null;

            Type type = Type.GetType(_typeNameProperty.stringValue);
            if (type == null)
                return null;

            return new InheritingTypeSelectDropdown.Item(_useFullName ? type.FullName : type.Name, type.Name, type.FullName, type.AssemblyQualifiedName);
        }

        private GUIContent GetDropdownButtonLabel(AdvancedDropdownItem value)
        {
            if (value is not InheritingTypeSelectDropdown.Item item || !item.isValid)
            {
                return new GUIContent("No State Set");
            }

            string label = item.typeName;
            string tooltip = item.assemblyQualifiedName;

            return new GUIContent(label, tooltip);
        }

        private void EnsureFullyQualified(SerializedProperty property)
        {
            if(IsFullyQualified(property) == false)
            {
                FullyQualify(property);
            }
        }

        private bool IsFullyQualified(SerializedProperty property)
        {
            var p = property.FindPropertyRelative("_typeName");
            var typeName = p.stringValue;
            return Type.GetType(typeName) != null;
        }

        private void FullyQualify(SerializedProperty property)
        {
            var p = property.FindPropertyRelative("_typeName");
            var typeName = p.stringValue;
            var t = Type.GetType($"{typeName}, RoR2, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

            if (t != null) //Type found, fully qualify it.
            {
                p.stringValue = t.AssemblyQualifiedName;
                p.serializedObject.ApplyModifiedProperties();
            }
        }

        public SerializableEntityStateTypePropertyDrawer()
        {
            _useFullName = propertyDrawerPreferenceSettings.GetOrCreateSetting("useFullNameForItems", false);
        }
    }

    [Obsolete("The SerializableEntityStateTypeDrawer is Obsolete")]
    public class SerializableEntityStateTypeDrawer
    {
        public bool useFullName;
        public event Action<InheritingTypeSelectDropdown.Item> onTypeSelectedHandler;

        public void DoEditorGUI(Rect position, string assemblyQualifiedName, GUIContent label)
        {
            var typeName = Type.GetType(assemblyQualifiedName)?.Name ?? "No State Set";

            var prefixRect = EditorGUI.PrefixLabel(position, label);

            if (EditorGUI.DropdownButton(prefixRect, new GUIContent(typeName, assemblyQualifiedName), FocusType.Passive))
            {
                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), typeof(EntityState), useFullName);
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

                var dropdown = new InheritingTypeSelectDropdown(new AdvancedDropdownState(), typeof(EntityState), useFullName);
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
    }
}