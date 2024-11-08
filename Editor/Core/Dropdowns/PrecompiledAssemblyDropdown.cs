using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RoR2.Editor
{
    internal class PrecompiledAssemblyDropdown : AdvancedDropdown
    {
        public string rootItemKey = "Precompiled Assemblies";
        public event Action<Item> onItemSelected;
        private static PrecompiledAssemblyData[] _precompiledAssemblyDatas = Array.Empty<PrecompiledAssemblyData>();

        protected override AdvancedDropdownItem BuildRoot()
        {
            if (_precompiledAssemblyDatas.Length == 0)
                CreateData();

            var items = new Dictionary<string, Item>();
            var rootItem = new Item(rootItemKey, rootItemKey, rootItemKey);
            items.Add(rootItemKey, rootItem);
            items.Add("None", new Item("None", string.Empty, string.Empty));
            foreach (var data in _precompiledAssemblyDatas)
            {
                var assemblyName = data.name;
                var itemPath = FileUtil.GetProjectRelativePath(data.path);
                while (true)
                {
                    var lastDashIndex = itemPath.LastIndexOf('/');
                    if (!items.ContainsKey(itemPath))
                    {
                        var wea = lastDashIndex == -1 ? itemPath : itemPath.Substring(lastDashIndex + 1);
                        var item = new Item(wea, assemblyName, data.path);
                        items.Add(itemPath, item);
                    }

                    if (itemPath.IndexOf('/') == -1) break;

                    itemPath = itemPath.Substring(0, lastDashIndex);
                }
            }

            foreach (var item in items)
            {
                if (item.Key == rootItemKey)
                    continue;

                var itemFullName = item.Key;
                if (itemFullName.LastIndexOf('/') == -1)
                {
                    rootItem.AddChild(item.Value);
                }
                else
                {
                    var parentName = itemFullName.Substring(0, itemFullName.LastIndexOf('/'));
                    items[parentName].AddChild(item.Value);
                }
            }

            return rootItem;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            onItemSelected?.Invoke((Item)item);
        }

        new public void Show(Rect rect)
        {
            var minSize = minimumSize;
            minSize.y = rect.height * 10;
            minimumSize = minSize;
            base.Show(rect);
        }

        private static PrecompiledAssemblyData[] CreateData()
        {
            List<PrecompiledAssemblyData> result = new List<PrecompiledAssemblyData>();
            foreach (var assemblyName in CompilationPipeline.GetPrecompiledAssemblyNames())
            {
                result.Add(new PrecompiledAssemblyData
                {
                    name = assemblyName,
                    path = CompilationPipeline.GetPrecompiledAssemblyPathFromAssemblyName(assemblyName)
                });
            }
            return result.ToArray();
        }

        static PrecompiledAssemblyDropdown()
        {
            _precompiledAssemblyDatas = CreateData();
        }

        public PrecompiledAssemblyDropdown(AdvancedDropdownState state) : base(state)
        {
        }

        public class Item : AdvancedDropdownItem
        {
            public string assemblyName { get; }
            public string assemblyPath { get; }

            public Item(string displayName, string assemblyName, string assemblyPath) : base(displayName)
            {
                this.assemblyName = assemblyName;
                this.assemblyPath = assemblyPath;
            }
        }
        private struct PrecompiledAssemblyData
        {
            public string name;
            public string path;
        }
    }
}