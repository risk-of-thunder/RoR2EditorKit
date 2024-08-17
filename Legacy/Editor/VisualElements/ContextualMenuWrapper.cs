using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.VisualElements
{
    /// <summary>
    /// A ContextualMenuWrapper is a VisualElement that is used in conjunction with the <see cref="ContextMenuHelper"/> to create Visible and Accessible ContextualMenus for your VisualElements.
    /// <para>By itself is just a wrapper of fields and properties that the <see cref="ContextMenuHelper"/> uses to encapsulate an existing VisualElement with the ContextMenuIcon</para>
    /// </summary>
    public class ContextualMenuWrapper : VisualElement
    {
        public override VisualElement contentContainer => _contentContainer;
        public VisualElement _contentContainer;
        /// <summary>
        /// The Icon for the VisualElement that contains the ContextMenus, by default this equates to RoR2EditorKit's Icon
        /// </summary>
        public Texture2D ContextMenuIcon
        {
            get
            {
                return IconElement.style.backgroundImage.value.texture;
            }
            set
            {
                IconElement.style.backgroundImage = new StyleBackground(value);
            }
        }
        /// <summary>
        /// The VisualElement that contains the ContextMenu of this ContextualMenuWrapper
        /// </summary>
        public VisualElement IconElement { get; private set; }

        /// <summary>
        /// Initializes a new ContextualMenuWrapper, useful if you need to create a wrapper beforehand before creating context menus using the <see cref="ContextMenuHelper"/>
        /// </summary>
        public ContextualMenuWrapper()
        {
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(GetType().Name, this, VisualElementUtil.ValidateUXMLPath);
            _contentContainer = this.Q<VisualElement>("content");
            IconElement = this.Q<VisualElement>("icon");
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        }

    }
}