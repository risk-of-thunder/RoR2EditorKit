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
    /// <summary>
    /// Data that defines a ContextMenu that will be used with the <see cref="ContextMenuHelper"/>
    /// <para>This data replaces the now deprecated ContextMenuData found in <see cref="RoR2EditorKit.Inspectors.ContextMenuData"/></para>
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

        /// <summary>
        /// Constructor for ContextMenuData
        /// <para>Sets ActionStatusCheck to normal and uses the RoR2EK icon for the contextual menu icon</para>
        /// </summary>
        public ContextMenuData(string name, Action<DropdownMenuAction> action, object userData = null)
        {
            menuAction = action;
            menuName = name;
            actionStatusCheck = (_) => DropdownMenuAction.Status.Normal;
            this.userData = userData;
            contextualMenuIcon = Constants.AssetGUIDS.QuickLoad<Texture2D>(Constants.AssetGUIDS.iconGUID);
        }

        /// <summary>
        /// Constructor for ContextMenuData
        /// <para>Sets ActionStatusCheck to normal</para>
        /// </summary>
        public ContextMenuData(string name, Action<DropdownMenuAction> action, Texture2D contextualMenuIcon, object userData = null)
        {
            menuAction = action;
            menuName = name;
            actionStatusCheck = (_) => DropdownMenuAction.Status.Normal;
            this.userData = userData;
            this.contextualMenuIcon = contextualMenuIcon;
        }

        /// <summary>
        /// Constructor for ContextMenuData
        /// <para>uses the RoR2EK icon for the contextual menu icon</para>
        /// </summary>
        public ContextMenuData(string name, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> statusCheck, object userData = null)
        {
            menuAction = action;
            menuName = name;
            actionStatusCheck = statusCheck;
            this.userData = userData;
            contextualMenuIcon = Constants.AssetGUIDS.QuickLoad<Texture2D>(Constants.AssetGUIDS.iconGUID);
        }

        /// <summary>
        /// Constructor for ContextMenuData
        /// </summary>
        public ContextMenuData(string name, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> statusCheck, Texture2D contextualMenuIcon, object userData = null)
        {
            menuAction = action;
            menuName = name;
            actionStatusCheck = statusCheck;
            this.userData = userData;
            this.contextualMenuIcon = contextualMenuIcon;
        }
    }

    /// <summary>
    /// The ContextMenuHelper is a class that allows for management of ContextMenus of VisualElements using a <see cref="ContextualMenuWrapper"/>
    /// <para>It's main method, <see cref="AddSimpleContextMenu(VisualElement, ContextMenuData)"/> allows you to give a specific VisualElement a ContextMenu, and have a special Icon that will be the clickable element that'll show the field's ContextMenu.</para>
    /// <para>This allows the Element to be visible, and have a clear way of telling the end user that it has a context menu, alongside being easy to find and click.</para>
    /// </summary>
    public static class ContextMenuHelper
    {
        private static FixedConditionalWeakTable<VisualElement, List<ContextMenuData>> elementToData = new FixedConditionalWeakTable<VisualElement, List<ContextMenuData>>();

        private static DropdownMenuAction.Status DefaultStatusCheck(DropdownMenuAction action) => DropdownMenuAction.Status.Normal;

        /// <summary>
        /// A custom way of adding a ContextMenu, it is incredibly recommended to use this method instead of manually adding a Manipulator, as this method will create a little icon that will be the clickable element for the context menu 
        /// </summary>
        /// <param name="element">The element that will be modified to be held by a ContextualMenuWrapper, this element will be removed from the hierarchy, parented to a ContextualMenuWrapper, and then the Wrapper will be inserted in the element's original position, preserving its position in the hierarchy and adding the icon that will contain the field's context menu.</param>
        /// <param name="data">The data for the context menu.</param>
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
                    populateEvent.menu.AppendAction(data.menuName, data.menuAction, data.actionStatusCheck ?? DefaultStatusCheck, data.userData);
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