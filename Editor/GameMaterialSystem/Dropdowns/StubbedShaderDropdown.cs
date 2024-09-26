using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RoR2.Editor.GameMaterialSystem
{
    public class StubbedShaderDropdown : AdvancedDropdown
    {
        private string rootItemKey = "Select Stubbed Shader";

        public event Action<Item> onShaderSelected;


        protected override AdvancedDropdownItem BuildRoot()
        {
            ReadOnlyCollection<SerializableShaderWrapper> shaders = GameMaterialSystemSettings.instance.stubbedShaders;

            var items = new Dictionary<string, Item>();
            var rootItem = new Item(rootItemKey, null);
            items.Add(rootItemKey, rootItem);

            for (int i = 0; i < shaders.Count; i++)
            {
                SerializableShaderWrapper shaderWrapper = shaders[i];
                var shader = shaderWrapper.shader;
                if (!shader)
                    continue;

                string shaderFullName = shader.name;
                while(true)
                {
                    var lastDashIndex = shaderFullName.LastIndexOf('/');
                    if(!items.ContainsKey(shaderFullName))
                    {
                        var shaderName = lastDashIndex == -1 ? shaderFullName : shaderFullName.Substring(lastDashIndex + 1);
                        var item = new Item(shaderName, i);

                        items.Add(shaderFullName, item);
                    }

                    if (shaderFullName.IndexOf('/') == -1)
                        break;

                    shaderFullName = shaderFullName.Substring(0, lastDashIndex);
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
            onShaderSelected?.Invoke((Item)item);
        }

        public StubbedShaderDropdown(AdvancedDropdownState state, Vector2? size) : base(state)
        {
            if(size.HasValue)
                minimumSize = size.Value;
        }

        public class Item : AdvancedDropdownItem
        {
            public SerializableShaderWrapper shader
            {
                get
                {
                    if(_index.HasValue)
                    {
                        return GameMaterialSystemSettings.instance.stubbedShaders[_index.Value];
                    }
                    return null;
                }
            }
            private int? _index;
            public Item(string name, int? index) : base(name)
            {
                _index = index;
            }
        }
    }
}
