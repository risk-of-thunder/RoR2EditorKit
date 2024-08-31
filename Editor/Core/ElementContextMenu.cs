using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public static class ElementContextMenu
    {
        private static FixedConditionalWeakTable<ContextMenuWrapperElement, List<ContextMenuData>> _wrapperToData = new FixedConditionalWeakTable<ContextMenuWrapperElement, List<ContextMenuData>>();
        private static DropdownMenuAction.Status DefaultStatusCheck(DropdownMenuAction action) => DropdownMenuAction.Status.Normal;

        public static void AddSimpleContextMenu(this VisualElement element, ContextMenuData data)
        {
            ContextMenuWrapperElement wrapper = WrapElement(element, data);
            var datas = _wrapperToData.GetValue(wrapper, CreateNewEntry);
            if (!datas.Contains(data))
            {
                datas.Add(data);
            }
        }

        private static List<ContextMenuData> CreateNewEntry(ContextMenuWrapperElement element)
        {
            var manipulator = new ContextualMenuManipulator(x => CreateMenu(element, x));
            element.iconElement.AddManipulator(manipulator);
            return new List<ContextMenuData>();
        }

        internal static void OnContextualMenuWrapperElementDestroyed(ContextMenuWrapperElement element) => _wrapperToData.Remove(element); 

        private static ContextMenuWrapperElement WrapElement(VisualElement elementToWrap, ContextMenuData data)
        {
            ContextMenuWrapperElement wrapper = null;

            if (IsElementWrapped(elementToWrap, out var w))
            {
                wrapper = w;
                return wrapper;
            }

            if(elementToWrap.parent == null)
            {
                DoWrap();
                return wrapper;
            }

            var parent = elementToWrap.parent;
            int originalIndex = parent.IndexOf(elementToWrap);
            elementToWrap.RemoveFromHierarchy();

            DoWrap();

            parent.Insert(originalIndex, wrapper);
            return wrapper;

            void DoWrap()
            {
                wrapper = new ContextMenuWrapperElement();
                if(data.contextualMenuIcon)
                {
                    wrapper.contextMenuIcon = data.contextualMenuIcon;
                }

                elementToWrap.style.flexGrow = 1f;
                elementToWrap.style.flexShrink = 0;
                wrapper.Add(elementToWrap);
            }
        }

        private static bool IsElementWrapped(VisualElement element, out ContextMenuWrapperElement wrapper)
        {
            if(element?.parent is ContextMenuWrapperElement wrapperElement)
            {
                wrapper = wrapperElement;
                return true;
            }
            wrapper = null;
            return false;
        }

        private static void CreateMenu(ContextMenuWrapperElement wrapperElement, ContextualMenuPopulateEvent evt)
        {
            if(_wrapperToData.TryGetValue(wrapperElement, out var datas))
            {
                foreach(var data in datas)
                {
                    evt.menu.AppendAction(data.menuName, data.menuAction, data.actionStatusCheck ?? DefaultStatusCheck, data.userData);
                }
            }
        }
    }
    public struct ContextMenuData
    {
        public string menuName;
        public Action<DropdownMenuAction> menuAction;
        public Func<DropdownMenuAction, DropdownMenuAction.Status> actionStatusCheck;
        public object userData;
        public Texture2D contextualMenuIcon;
    }
}