using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    public static class AddressablesPathPropertyDrawerPickerHelper
    {
        public sealed class DrawerArgs
        {
            public bool useFullPathForItems;
            public string filter;
            public Type[] allowedTypes;
            public string currentGUIDValue;

            public Action<AddressablesPathDropdown.Item> onItemSelected;

            //GUI related
            public GUIContent label;

            //These are specific to R2API's AddressableComponentRequirementAttribute.
            public Type requiredComponentType;
            public bool searchComponentInChildren;
        }

        private static float _standardPropertyHeight => EditorGUIUtility.singleLineHeight;

        //Can get a rect in an EditorGUILayout context by using it's GetControlRect method.
        public static void DrawPicker(Rect rect, DrawerArgs args)
        {
            if(args.allowedTypes == null || args.allowedTypes.Length == 0)
            {
                EditorGUI.LabelField(rect, "No allowed types detected.");
                return;
            }

            //Draw the label for the main control
            var guiContent = args.label;
            var rectForDropdownControl = EditorGUI.PrefixLabel(rect, guiContent);

            //Draw the filter label.
            var filterRect = new Rect(rect.x + 32, rect.y + _standardPropertyHeight, rect.width - 32, _standardPropertyHeight);
            args.filter = EditorGUI.TextField(filterRect, "Filter: ", args.filter);

            //Draw the main control
            GUIContent dropdownButtonLabel = GetDropdownButtonLabel(args.currentGUIDValue);
            if(EditorGUI.DropdownButton(rectForDropdownControl, dropdownButtonLabel, FocusType.Passive))
            {
                Type requiredComponentType = args.requiredComponentType;
                bool searchInChildren = args.searchComponentInChildren;

                AddressablesPathDropdown dropdown = new AddressablesPathDropdown(new AdvancedDropdownState(),
                    args.useFullPathForItems,
                    args.filter,
                    requiredComponentType,
                    searchInChildren,
                    args.allowedTypes);

                dropdown.onItemSelected += args.onItemSelected;

                dropdown.Show(rectForDropdownControl);
            }
        }

        private static GUIContent GetDropdownButtonLabel(string currentGUIDValue)
        {
            string label = "";
            string tooltip = "";

            if(string.IsNullOrEmpty(currentGUIDValue))
            {
                label = "None";
                tooltip = "";
                return new GUIContent(label, tooltip);
            }

            string fullAssetPath = AddressablesPathDictionary.instance.GetPathFromGUID(currentGUIDValue);
            string onlyAssetName = fullAssetPath.Substring(fullAssetPath.LastIndexOf('/') + 1);

            label = onlyAssetName;
            tooltip = fullAssetPath;

            return new GUIContent(label, tooltip);
        }
    }
}