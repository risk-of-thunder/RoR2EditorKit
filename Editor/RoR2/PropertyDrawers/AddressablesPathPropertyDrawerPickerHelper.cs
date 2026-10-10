using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    /// <summary>
    /// Helper class for drawing the AddressablesPathDictionary picker dropdown.
    /// <br></br>
    /// The following things use this class:
    /// <list type="bullet">
    /// <item><see cref="BaseGameAssetReferenceTDrawer"/></item>
    /// <item>The property drawer for <see cref="R2API.AddressReferencedAssets.AddressReferencedAsset"/></item>
    /// <item>The VisualElement control for <see cref="RoR2.AsyncManagement.EntityStateGameObject"/> in <see cref="EntityStateConfiguration"/></item>
    /// </list>
    /// <para></para>
    /// You can use this class to implement the drawer on your own UI's.
    /// </summary>
    public static class AddressablesPathPropertyDrawerPickerHelper
    {
        /// <summary>
        /// Arguments for the drawer
        /// </summary>
        public sealed class DrawerArgs
        {
            /// <summary>
            /// If true, the Dropdown will use the full asset path for the items
            /// </summary>
            public bool useFullPathForItems;
            /// <summary>
            /// A filter for the query, only assets with this substring in it's path will be included. This filter is updated by the drawer itself.
            /// </summary>
            public string filter;
            /// <summary>
            /// The types that the dropdown will display
            /// </summary>
            public Type[] allowedTypes;
            /// <summary>
            /// The current GUID value for the dropdown, this doesnt get updated if an item is selected
            /// </summary>
            public string currentGUIDValue;

            /// <summary>
            /// An action to invoke when an item is selected
            /// </summary>
            public Action<AddressablesPathDropdown.Item> onItemSelected;

            //GUI related
            /// <summary>
            /// The label for the picker field
            /// </summary>
            public GUIContent label;

            //These are specific to R2API's AddressableComponentRequirementAttribute.
            /// <summary>
            /// A required component that the GameObject must have, only relevant if <see cref="allowedTypes"/> is of length 1 and it's only value is GameObject
            /// </summary>
            public Type requiredComponentType;
            /// <summary>
            /// If true, <see cref="requiredComponentType"/> will be searched with <see cref="Component.GetComponentInChildren(Type)"/>
            /// </summary>
            public bool searchComponentInChildren;
        }

        private static float _standardPropertyHeight => EditorGUIUtility.singleLineHeight;

        //Can get a rect in an EditorGUILayout context by using it's GetControlRect method.
        /// <summary>
        /// Draws the AddressablesPathDictionary picker drawer.
        /// </summary>
        /// <param name="rect">The rect for the drawer, if you're calling this inside an IMGUIContainer, Inspector or Window, you can call <see cref="EditorGUILayout.GetControlRect(bool, float, GUILayoutOption[])"/> to retrieve the value you need.</param>
        /// <param name="args">The arguments for the picker</param>
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