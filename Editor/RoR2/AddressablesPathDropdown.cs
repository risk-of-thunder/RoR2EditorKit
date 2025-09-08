using HG;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RoR2.Editor
{
    /// <summary>
    /// Represents a Dropdown that can be used to obtain an Addressable GUID from the base game, displaying them as Addressable Paths for ease of use.
    /// </summary>
    public class AddressablesPathDropdown : AdvancedDropdown
    {
        private string rootItemKey;
        private Type[] requiredTypes;

        /// <summary>
        /// This event is fired when an Item is selected.
        /// </summary>
        public event Action<Item> onItemSelected;

        /// <summary>
        /// If true, the full path will be used as an individual item's name.
        /// </summary>
        public bool useFullPathAsItemName { get; }

        protected override AdvancedDropdownItem BuildRoot()
        {
            ReadOnlyArray<string> keys = AddressablesPathDictionary.GetInstance().GetAllPathsOfTypes(requiredTypes);

            var items = new Dictionary<string, Item>();
            var rootItem = new Item(rootItemKey, rootItemKey);
            items.Add(rootItemKey, rootItem);

            items.Add("None", new Item("None", string.Empty));
            foreach(var assetPath in keys)
            {
                var fullPath = assetPath;
                while(true)
                {
                    var lastDashIndex = fullPath.LastIndexOf('/');
                    if(!items.ContainsKey(fullPath))
                    {
                        var path = lastDashIndex == -1 ? fullPath : fullPath.Substring(lastDashIndex + 1);
                        var item = new Item(useFullPathAsItemName ? fullPath : path, fullPath);
                        items.Add(fullPath, item);
                    }

                    if (fullPath.IndexOf('/') == -1)
                        break;

                    fullPath = fullPath.Substring(0, lastDashIndex);
                }
            }

            foreach(var item in items)
            {
                if (item.Key == rootItemKey)
                    continue;

                var fullName = item.Key;
                if(fullName.LastIndexOf('/') == -1)
                {
                    rootItem.AddChild(item.Value);
                }
                else
                {
                    var parentName = fullName.Substring(0, fullName.LastIndexOf('/'));
                    items[parentName].AddChild(item.Value);
                }
            }
            return rootItem;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onItemSelected?.Invoke((Item)item);
        }

        /// <summary>
        /// Constructor with no type checking. You're strongly encouraged to use <see cref="AddressablesPathDropdown.AddressablesPathDropdown(AdvancedDropdownState, bool, Type)"/> instead.
        /// </summary>
        /// <param name="state">The state of the dropdown, can pass an empty state.</param>
        /// <param name="useFullPathAsItemName">If true, the full path will be used as an individual item's name.</param>
        public AddressablesPathDropdown(AdvancedDropdownState state, bool useFullPathAsItemName) : this(state, useFullPathAsItemName, requiredType: null)
        {

        }

        /// <summary>
        /// Constructor with Type checking, this is the recommended constructor.
        /// </summary>
        /// <param name="state">The state of the dropdown, can pass an empty state.</param>
        /// <param name="useFullPathAsItemName">If true, the full path will be used as an individual item's name.</param>
        /// <param name="requiredType">The required type of the asset, this will filter the dropdown to only include assets of this type.</param>
        public AddressablesPathDropdown(AdvancedDropdownState state, bool useFullPathAsItemName, Type requiredType) : this(state, useFullPathAsItemName, new Type[] {requiredType})
        {

        }

        /// <summary>
        /// Constructor with multiple Type checking.
        /// </summary>
        /// <param name="state">The state of the dropdown, can pass an empty state.</param>
        /// <param name="useFullPathAsItemName">If true, the full path will be used as an individual item's name.</param>
        /// <param name="requiredType">The required types of the asset, this will filter the dropdown to only include assets of these types.</param>
        public AddressablesPathDropdown(AdvancedDropdownState state, bool useFullPathAsItemName, Type[] requiredTypes) : base(state)
        {
            rootItemKey = "Select Asset";
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
            this.requiredTypes = requiredTypes;
            this.useFullPathAsItemName = useFullPathAsItemName;
        }

        /// <summary>
        /// Represents an item within the dropdown.
        /// </summary>
        public class Item : AdvancedDropdownItem
        {
            /// <summary>
            /// The Addressable Path to the asset, you can use <see cref="AddressablesPathDictionary"/>'s GetGUIDFromPath to obtain the actual guid of the asset.
            /// </summary>
            public string assetPath { get; }

            internal Item(string displayName, string assetPath) : base(displayName)
            {
                this.assetPath = assetPath;
            }
        }
    }
}