using RoR2EditorKit.VisualElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit
{
    public struct ContextMenuData
    {
        public string menuName;
        public Action<DropdownMenuAction> menuAction;
        public Func<DropdownMenuAction, DropdownMenuAction.Status> actionStatusCheck;
        public object userData;
        public Texture2D contextualMenuIcon;

        public ContextMenuData(string name, Action<DropdownMenuAction> action, object userData = null)
        {
            menuAction = action;
            menuName = name;
            actionStatusCheck = (_) => DropdownMenuAction.Status.Normal;
            this.userData = userData;
            contextualMenuIcon = Constants.AssetGUIDS.QuickLoad<Texture2D>(Constants.AssetGUIDS.iconGUID);
        }

        public ContextMenuData(string name, Action<DropdownMenuAction> action, Texture2D contextualMenuIcon, object userData = null)
        {
            menuAction = action;
            menuName = name;
            actionStatusCheck = (_) => DropdownMenuAction.Status.Normal;
            this.userData = userData;
            this.contextualMenuIcon = contextualMenuIcon;
        }

        public ContextMenuData(string name, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> statusCheck, object userData = null)
        {
            menuAction = action;
            menuName = name;
            actionStatusCheck = statusCheck;
            this.userData = userData;
            contextualMenuIcon = Constants.AssetGUIDS.QuickLoad<Texture2D>(Constants.AssetGUIDS.iconGUID);
        }

        public ContextMenuData(string name, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> statusCheck, Texture2D contextualMenuIcon, object userData = null)
        {
            menuAction = action;
            menuName = name;
            actionStatusCheck = statusCheck;
            this.userData = userData;
            this.contextualMenuIcon = contextualMenuIcon;
        }
    }

    public static class ContextMenuHelper
    {
        private static FixedConditionalWeakTable<VisualElement, List<ContextMenuData>> elementToData = new FixedConditionalWeakTable<VisualElement, List<ContextMenuData>>();

        public static void AddSimpleContextMenu(this VisualElement element, ContextMenuData data)
        {
            ContextualMenuWrapper wrapper = PrepareElement(element, data);
            var datas = elementToData.GetValue(element, CreateNewEntry);
            if(!datas.Contains(data))
            {
                datas.Add(data);
            }
        }

        private static List<ContextMenuData> CreateNewEntry(VisualElement element)
        {
            ContextualMenuWrapper wrapper = (ContextualMenuWrapper)element.parent;
            var manipulator = new ContextualMenuManipulator(x => CreateMenu(element, x));
            wrapper.IconElement.AddManipulator(manipulator);
            return new List<ContextMenuData>();
        }
        private static void CreateMenu(VisualElement element, ContextualMenuPopulateEvent populateEvent)
        {
            if (elementToData.TryGetValue(element, out var datas))
            {
                foreach (ContextMenuData data in datas)
                {
                    populateEvent.menu.AppendAction(data.menuName, data.menuAction, data.actionStatusCheck, data.userData);
                }
            }
        }

        private static ContextualMenuWrapper PrepareElement(VisualElement originalElement, ContextMenuData data)
        {
            ContextualMenuWrapper wrapper = null;
            if (IsElementWrapped(originalElement, out wrapper))
            {
                return wrapper;
            }

            if (originalElement.parent == null)
            {
                wrapper = new ContextualMenuWrapper();
                if (data.contextualMenuIcon)
                    wrapper.ContextMenuIcon = data.contextualMenuIcon;
                wrapper.Add(originalElement);
                originalElement.style.flexGrow = new StyleFloat(1f);
                originalElement.style.flexShrink = new StyleFloat(0f);
                return wrapper;
            }

            var parent = originalElement.parent;
            int originalIndex = parent.IndexOf(originalElement);
            originalElement.RemoveFromHierarchy();

            wrapper = new ContextualMenuWrapper();
            if (data.contextualMenuIcon)
                wrapper.ContextMenuIcon = data.contextualMenuIcon;
            originalElement.style.flexGrow = new StyleFloat(1f);
            originalElement.style.flexShrink = new StyleFloat(0f);
            wrapper.Add(originalElement);

            parent.Insert(originalIndex, wrapper);
            return wrapper;
        }

        private static bool IsElementWrapped(VisualElement originalElement, out ContextualMenuWrapper wrapper)
        {
            if (originalElement?.parent is ContextualMenuWrapper)
            {
                wrapper = (ContextualMenuWrapper)originalElement.parent;
                return true;
            }
            wrapper = null;
            return false;
        }
    }
}