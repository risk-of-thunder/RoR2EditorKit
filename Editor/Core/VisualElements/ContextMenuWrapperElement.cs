using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// A <see cref="ContextMenuWrapperElement"/> is a VisualElement that's used in conjunction with the <see cref="ElementContextMenuHelper"/> to create visible and accessible context menus for your VisualElements.
    /// <para>By itself its just a wrapper of fields and properties that the <see cref="ElementContextMenuHelper"/> uses to encapsulate an existing VisualElement with the ContextMenuIcon</para>
    /// </summary>
    public class ContextMenuWrapperElement : VisualElement
    {
        public override VisualElement contentContainer => _contentContainer;
        public VisualElement _contentContainer;

        /// <summary>
        /// The Icon for the VisualElement that contains the ContextMenus, by default, this equates to R2EK's Icon
        /// </summary>
        public Texture2D contextMenuIcon
        {
            get
            {
                return iconElement.style.backgroundImage.value.texture;
            }
            set
            {
                iconElement.style.backgroundImage = new StyleBackground(value);
            }
        }

        /// <summary>
        /// The VisualElement that contains the ContextMenu of this ContextMenuWrapperElement
        /// </summary>
        public VisualElement iconElement { get; private set; }

        /// <summary>
        /// Initializes a new ContextMenuWrapperelement, useful if you need to crate a wrapper before creating context menus using the <see cref="ElementContextMenuHelper"/>
        /// </summary>
        public ContextMenuWrapperElement()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, this);
            _contentContainer = this.Q<VisualElement>("content");
            iconElement = this.Q<VisualElement>("icon");
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        }
    }
}
