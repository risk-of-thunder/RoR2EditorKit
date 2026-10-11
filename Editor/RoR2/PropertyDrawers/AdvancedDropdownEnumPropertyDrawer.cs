using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    /// <summary>
    /// The <see cref="AdvancedDropdownEnumPropertyDrawer{TEnum}"/> is an <see cref="IMGUIPropertyDrawer{T}"/> used to display an Enum field as an AdvancedDropdown rather than a regular Dropdown menu.
    /// <br></br>
    /// Thanks to this users can search for a specific enum value in the AdvancedDropdownWindow, expediting the query for a specific value, particularly useful with enums that have a large amount of entries.
    /// </summary>
    /// <typeparam name="TEnum">The type of enum that we're drawing</typeparam>
    public abstract class AdvancedDropdownEnumPropertyDrawer<TEnum> : IMGUIPropertyDrawer<TEnum> where TEnum : Enum
    {
        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using(new EditorGUI.PropertyScope(position, label, property))
            {
                var dropdownButtonRect = EditorGUI.PrefixLabel(position, label);
                if(EditorGUI.DropdownButton(dropdownButtonRect, GetEnumValueName(property), FocusType.Passive))
                {
                    var dropdown = new AdvancedDropdownEnum(new AdvancedDropdownState());
                    dropdown.onItemSelected += (item) =>
                    {
                        var enumValue = item.enumValue;
                        var enumName = Enum.GetName(typeof(TEnum), enumValue);
                        int index = -1;
                        for(int i = 0; i < property.enumNames.Length; i++)
                        {
                            var enumPropertyName = property.enumNames[i];
                            if(string.Equals(enumPropertyName, enumName))
                            {
                                index = i;
                                break;
                            }
                        }

                        property.enumValueIndex = index;
                        property.serializedObject.ApplyModifiedProperties();
                    };
                    dropdown.Show(position);
                }
            }
        }

        private GUIContent GetEnumValueName(SerializedProperty property)
        {
            var index = property.enumValueIndex;
            var enumName = property.enumNames[index];
            return new GUIContent(enumName);
        }

        private class AdvancedDropdownEnum : AdvancedDropdown
        {
            public AdvancedDropdownEnum(AdvancedDropdownState state) : base(state)
            {
            }

            public event Action<Item> onItemSelected;

            protected override AdvancedDropdownItem BuildRoot()
            {
                Item root = new Item(ObjectNames.NicifyVariableName(typeof(TEnum).Name), default);

                TEnum[] enumValues = (TEnum[])Enum.GetValues(typeof(TEnum));
                foreach(TEnum enumValue in enumValues)
                {
                    root.AddChild(new Item(Enum.GetName(typeof(TEnum), enumValue), enumValue));
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
                public Item(string name, TEnum enumValue) : base(name)
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