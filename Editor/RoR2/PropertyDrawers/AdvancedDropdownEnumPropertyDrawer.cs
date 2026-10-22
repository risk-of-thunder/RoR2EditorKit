using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.PropertyDrawers
{
    /// <summary>
    /// The <see cref="AdvancedDropdownEnumPropertyDrawer{TEnum}"/> is an <see cref="IMGUIPropertyDrawer{T}"/> used to display an Enum field as an AdvancedDropdown rather than a regular Dropdown menu.
    /// <br></br>
    /// Thanks to this users can search for a specific enum value in the AdvancedDropdownWindow, expediting the query for a specific value, particularly useful with enums that have a large amount of entries.
    /// </summary>
    /// <typeparam name="TEnum">The type of enum that we're drawing</typeparam>
    public abstract class AdvancedDropdownEnumPropertyDrawer<TEnum> : ExtendedPropertyDrawer<TEnum> where TEnum : Enum
    {
        #region VisualElement
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            AdvancedDropdownElement dropdownElement = new AdvancedDropdownElement(property.GetGUIContent(), GetCurrentValue(property), (element) =>
            {
                AdvancedDropdownEnum dropdown = new AdvancedDropdownEnum(new AdvancedDropdownState());
                dropdown.onItemSelected += element.OnItemSelected;
                return dropdown;
            });
            dropdownElement.showMixedValue = property.hasMultipleDifferentValues;
            dropdownElement.RegisterValueChangedCallback((evt) =>
            {
                var newValue = evt.newValue;
                if (newValue is AdvancedDropdownEnum.Item item)
                {
                    var enumValue = item.enumValue;
                    var enumName = Enum.GetName(typeof(TEnum), enumValue);
                    int index = -1;
                    for (int i = 0; i < property.enumNames.Length; i++)
                    {
                        var enumPropertyName = property.enumNames[i];
                        if (string.Equals(enumPropertyName, enumName))
                        {
                            index = i;
                            break;
                        }
                    }

                    property.enumValueIndex = index;
                    property.serializedObject.ApplyModifiedProperties();
                }
            });
            return dropdownElement;
        }
        #endregion

        #region IMGUI
        private struct NewPropertyValueContainer
        {
            public SerializedProperty targetProperty;
            public int newEnumIndexValue;
        }

        private NewPropertyValueContainer? newPropertyValue;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
            using (new EditorGUI.PropertyScope(position, label, property))
            {
                var dropdownButtonRect = EditorGUI.PrefixLabel(position, label);
                if (EditorGUI.DropdownButton(dropdownButtonRect, new GUIContent(GetCurrentValue(property).name), FocusType.Passive))
                {
                    var dropdown = new AdvancedDropdownEnum(new AdvancedDropdownState());
                    dropdown.onItemSelected += (item) =>
                    {
                        var enumValue = item.enumValue;
                        var enumName = Enum.GetName(typeof(TEnum), enumValue);
                        int index = -1;
                        for (int i = 0; i < property.enumNames.Length; i++)
                        {
                            var enumPropertyName = property.enumNames[i];
                            if (string.Equals(enumPropertyName, enumName))
                            {
                                index = i;
                                break;
                            }
                        }

                        newPropertyValue = new NewPropertyValueContainer { newEnumIndexValue = index, targetProperty = property };
                    };

                    dropdown.Show(position);
                }

                if (newPropertyValue.HasValue && SerializedProperty.EqualContents(newPropertyValue.Value.targetProperty, property))
                {
                    newPropertyValue.Value.targetProperty.enumValueIndex = newPropertyValue.Value.newEnumIndexValue;
                    newPropertyValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
        #endregion

        private AdvancedDropdownEnum.Item GetCurrentValue(SerializedProperty property)
        {
            TEnum currentValue = (TEnum)Enum.Parse(typeof(TEnum), property.enumNames[property.enumValueIndex]);
            return new AdvancedDropdownEnum.Item(currentValue);
        }

        private class AdvancedDropdownEnum : AdvancedDropdown
        {
            public AdvancedDropdownEnum(AdvancedDropdownState state) : base(state)
            {
            }

            public event Action<Item> onItemSelected;

            protected override AdvancedDropdownItem BuildRoot()
            {
                Item root = new Item(default, ObjectNames.NicifyVariableName(typeof(TEnum).Name));

                TEnum[] enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
                foreach(TEnum enumValue in enumValues)
                {
                    root.AddChild(new Item(enumValue));
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                base.ItemSelected(item);
                onItemSelected?.Invoke((Item)item);
            }

            public class Item : AdvancedDropdownItem
            {
                public Item(TEnum enumValue) : this(enumValue, enumValue.ToString()) { }
                public Item(TEnum enumValue, string name) : base(name)
                {
                    this.enumValue = enumValue;
                }

                public TEnum enumValue { get; }
            }
        }
    }

    [CustomPropertyDrawer(typeof(ItemTag))]
    public sealed class ItemTagPropertyDrawer : AdvancedDropdownEnumPropertyDrawer<ItemTag>
    {

    }
}