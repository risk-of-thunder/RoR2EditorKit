using System.Collections.Generic;
using System;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using System.Linq;

namespace RoR2.Editor
{
    public class InheritingTypeSelectDropdown : AdvancedDropdown
    {
        private string rootItemKey;
        public event Action<Item> onItemSelected;

        public Type requiredBaseType { get; set; }

        protected override AdvancedDropdownItem BuildRoot()
        {
            IEnumerable<Type> types = requiredBaseType != null ? TypeCache.GetTypesDerivedFrom(requiredBaseType) : ReflectionUtils.allTypes;


            var items = new Dictionary<string, Item>();
            var rootItem = new Item(rootItemKey, rootItemKey, rootItemKey, rootItemKey);
            items.Add(rootItemKey, rootItem);

            items.Add("None", new Item("None", string.Empty, string.Empty, string.Empty));
            foreach (var assemblyQualifiedName in types.Select(x => x.AssemblyQualifiedName).OrderBy(x => x))
            {
                var itemFullName = assemblyQualifiedName.Split(',')[0];
                while (true)
                {
                    var lastDotIndex = itemFullName.LastIndexOf('.');
                    if (!items.ContainsKey(itemFullName))
                    {
                        var typeName =
                            lastDotIndex == -1 ? itemFullName : itemFullName.Substring(lastDotIndex + 1);
                        var item = new Item(typeName, typeName, itemFullName, assemblyQualifiedName);
                        items.Add(itemFullName, item);
                    }

                    if (itemFullName.IndexOf('.') == -1) break;

                    itemFullName = itemFullName.Substring(0, lastDotIndex);
                }
            }

            foreach (var item in items)
            {
                if (item.Key == rootItemKey)
                    continue;

                var fullName = item.Key;
                if (fullName.LastIndexOf('.') == -1)
                {
                    rootItem.AddChild(item.Value);
                }
                else
                {
                    var parentName = fullName.Substring(0, fullName.LastIndexOf('.'));
                    items[parentName].AddChild(item.Value);
                }
            }

            return rootItem;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onItemSelected?.Invoke((Item)item);
        }

        public InheritingTypeSelectDropdown(AdvancedDropdownState state) : this(state, null) { }
        public InheritingTypeSelectDropdown(AdvancedDropdownState state, Type requiredBaseType) : base(state)
        {
            this.requiredBaseType = requiredBaseType;
            rootItemKey = requiredBaseType.Name;
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
        }

        public class Item : AdvancedDropdownItem
        {
            public string typeName { get; }
            public string fullName { get; }
            public string assemblyQualifiedName { get; }

            public Item(string displayName, string typeName, string fullName, string assemblyQualifiedName) : base(
    displayName)
            {
                this.typeName = typeName;
                this.fullName = fullName;
                this.assemblyQualifiedName = assemblyQualifiedName;
            }
        }
    }
}