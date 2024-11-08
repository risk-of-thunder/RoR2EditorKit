using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="ElementContextMenuHelper"/> is a class that allows for management of ContextMenus of VisualElements using a <see cref="ContextMenuWrapperElement"/>.
    /// 
    /// <para>It's main method,  <see cref="AddSimpleContextMenu(VisualElement, ContextMenuData)"/>, allows you to give a specific VisualElement a ContextMenu, and have a special Icon that will be the clickable element that'll show the field's ContextMenu</para>
    /// <br>This allows the Element to be visible, and have a clear way of telling the end user that it has a context menu, alongside being easy to find and click.</br>
    /// </summary>
    public static class ElementContextMenuHelper
    {
        private static FixedConditionalWeakTable<ContextMenuWrapperElement, List<ContextMenuData>> _wrapperToData = new FixedConditionalWeakTable<ContextMenuWrapperElement, List<ContextMenuData>>();
        private static DropdownMenuAction.Status DefaultStatusCheck(DropdownMenuAction action) => DropdownMenuAction.Status.Normal;

        /// <summary>
        /// A custom way of adding a ContextMenu, it is incredibly recommended to use this method instead of manually adding a Manipulator, as this method will create a little icon that will be the clickable element for the context menu 
        /// </summary>
        /// <param name="element">The element that will be modified to be held by a ContextualMenuWrapper, this element will be removed from the hierarchy, parented to a ContextualMenuWrapper, and then the Wrapper will be inserted in the element's original position, preserving its position in the hierarchy and adding the icon that will contain the field's context menu.</param>
        /// <param name="data">The data for the context menu.</param>
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

        private static ContextMenuWrapperElement WrapElement(VisualElement elementToWrap, ContextMenuData data)
        {
            ContextMenuWrapperElement wrapper = null;

            if (IsElementWrapped(elementToWrap, out var w))
            {
                wrapper = w;
                return wrapper;
            }

            if (elementToWrap.parent == null)
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
                if (data.contextualMenuIcon)
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
            if (element?.parent is ContextMenuWrapperElement wrapperElement)
            {
                wrapper = wrapperElement;
                return true;
            }
            wrapper = null;
            return false;
        }

        private static void CreateMenu(ContextMenuWrapperElement wrapperElement, ContextualMenuPopulateEvent evt)
        {
            if (_wrapperToData.TryGetValue(wrapperElement, out var datas))
            {
                foreach (var data in datas)
                {
                    evt.menu.AppendAction(data.menuName, data.menuAction, data.actionStatusCheck ?? DefaultStatusCheck, data.userData);
                }
            }
        }
    }

    /// <summary>
    /// Data that defines a ContextMenu that will be used with the <see cref="ElementContextMenuHelper"/>
    /// </summary>
    public struct ContextMenuData
    {
        /// <summary>
        /// The menu name for this context menu, you can group these with /, IE: MyCustomMenu/DoSomething
        /// </summary>
        public string menuName;
        /// <summary>
        /// The action that runs when the context menu is clicked.
        /// </summary>
        public Action<DropdownMenuAction> menuAction;
        /// <summary>
        /// A status check to see the status of the context menu
        /// </summary>
        public Func<DropdownMenuAction, DropdownMenuAction.Status> actionStatusCheck;
        /// <summary>
        /// Optional data to pass for the menu to function
        /// </summary>
        public object userData;
        /// <summary>
        /// An texture for the icon that will be displayed next to the element that will get the context menu.
        /// <para>The icon should ideally be a multiple of 256 </para>
        /// </summary>
        public Texture2D contextualMenuIcon;
    }
}