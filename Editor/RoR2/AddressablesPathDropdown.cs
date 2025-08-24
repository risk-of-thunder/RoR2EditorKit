using HG;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RoR2.Editor
{
    public class AddressablesPathDropdown : AdvancedDropdown
    {
        private string rootItemKey;
        private Type requiredType;
        public event Action<Item> onItemSelected;

        public bool useFullPathAsItemName { get; }

        protected override AdvancedDropdownItem BuildRoot()
        {
            ReadOnlyArray<string> keys = AddressablesPathDictionary.GetInstance().GetAllKeysOfType(requiredType);

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
        /// Constructor with no type checking.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="useFullPathAsItemName"></param>
        public AddressablesPathDropdown(AdvancedDropdownState state, bool useFullPathAsItemName) : this(state, useFullPathAsItemName, null)
        {

        }

        public AddressablesPathDropdown(AdvancedDropdownState state, bool useFullPathAsItemName, Type requiredType) : base(state)
        {
            rootItemKey = "Select Asset";
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
            this.requiredType = requiredType;
            this.useFullPathAsItemName = useFullPathAsItemName;
        }

        public class Item : AdvancedDropdownItem
        {
            public string assetPath { get; }

            internal Item(string displayName, string assetPath) : base(displayName)
            {
                this.assetPath = assetPath;
            }
        }
    }
}