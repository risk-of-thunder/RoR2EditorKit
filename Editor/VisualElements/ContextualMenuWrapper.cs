using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.VisualElements
{
    public class ContextualMenuWrapper : VisualElement
    {
        public override VisualElement contentContainer => contentContainer;
        public VisualElement _contentContainer;
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

        public VisualElement IconElement { get; private set; }
        public ContextualMenuWrapper()
        {
            ThunderKit.Core.UIElements.TemplateHelpers.GetTemplateInstance(GetType().Name, this, (_) => true);
            _contentContainer = this.Q<VisualElement>("content");
            IconElement = this.Q<VisualElement>("icon");
            style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
        }

    }
}