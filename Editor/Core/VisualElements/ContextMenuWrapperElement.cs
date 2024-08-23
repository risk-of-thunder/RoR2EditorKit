using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public class ContextMenuWrapperElement : VisualElement
    {
        public override VisualElement contentContainer => _contentContainer;
        public VisualElement _contentContainer;

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

        public VisualElement iconElement { get; private set; }

        public ContextMenuWrapperElement()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, this);
            _contentContainer = this.Q<VisualElement>("content");
            iconElement = this.Q<VisualElement>("icon");
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        }

        ~ContextMenuWrapperElement()
        {
            ElementContextMenu.OnContextualMenuWrapperElementDestroyed(this);
        }
    }
}
