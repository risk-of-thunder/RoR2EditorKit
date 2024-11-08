using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="InheritingTypeSelectDropdown"/> is an <see cref="AdvancedDropdown"/> that can be used to select a specific Type that inherits from another type.
    /// 
    /// <br>These are used by the SerializableSystemType drawer and the SerializableStateType drawer</br>
    /// </summary>
    public class InheritingTypeSelectDropdown : AdvancedDropdown
    {
        private string rootItemKey;

        /// <summary>
        /// When the user clicks on an item, this action is fired
        /// </summary>
        public event Action<Item> onItemSelected;

        public bool useFullNameAsItemName { get; }

        /// <summary>
        /// The baseType to use as a filter
        /// </summary>
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
                        var item = new Item(useFullNameAsItemName ? itemFullName : typeName, typeName, itemFullName, assemblyQualifiedName);
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

        /// <summary>
        /// Constructor for the <see cref="InheritingTypeSelectDropdown"/> with no specified baseType
        /// </summary>
        public InheritingTypeSelectDropdown(AdvancedDropdownState state) : this(state, null) { }
        /// <summary>
        /// Constructor for the <see cref="InheritingTypeSelectDropdown"/> with a specified base type
        /// </summary>
        /// <param name="requiredBaseType">The base type to use as a filter</param>
        public InheritingTypeSelectDropdown(AdvancedDropdownState state, Type requiredBaseType) : this(state, requiredBaseType, false)
        {

        }

        public InheritingTypeSelectDropdown(AdvancedDropdownState state, Type requiredBaseType, bool useFullName) : base(state)
        {
            this.requiredBaseType = requiredBaseType;
            rootItemKey = requiredBaseType?.Name ?? "Select Type";
            var minSize = minimumSize;
            minSize.y = 200;
            minimumSize = minSize;
            useFullNameAsItemName = useFullName;
        }

        /// <summary>
        /// Represents a Type in the dropdown
        /// </summary>
        public class Item : AdvancedDropdownItem
        {
            /// <summary>
            /// The Type's <see cref="Type.Name"/>
            /// </summary>
            public string typeName { get; }

            /// <summary>
            /// The Type's <see cref="Type.FullName"/>
            /// </summary>
            public string fullName { get; }

            /// <summary>
            /// The Type's <see cref="Type.AssemblyQualifiedName"/>
            /// </summary>
            public string assemblyQualifiedName { get; }

            internal Item(string displayName, string typeName, string fullName, string assemblyQualifiedName) : base(
    displayName)
            {
                this.typeName = typeName;
                this.fullName = fullName;
                this.assemblyQualifiedName = assemblyQualifiedName;
            }
        }
    }
}