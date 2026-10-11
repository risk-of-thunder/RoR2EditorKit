using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.Deprecated
{
    [Obsolete(OBSOLETE_MESSAGE)]
    public abstract class ComponentInspector<T> : Inspector<T> where T : Component
    {
        private VisualElement toggleTemplateInstance;
        private Toggle _toggle;
        protected override void OnEnable()
        {
            base.OnEnable();
            toggleTemplateInstance = VisualElementTemplateDictionary.instance.GetTemplateInstance("ComponentInspectorEnableToggle");
            _toggle = toggleTemplateInstance.Q<Toggle>();
            _toggle.value = inspectorEnabled;
            _toggle.RegisterValueChangedCallback(cb =>
            {
                inspectorEnabled = cb.newValue;
            });
            onRootElementCleared += AddToggle;
        }

        private void AddToggle()
        {
            rootVisualElement.Add(toggleTemplateInstance);
        }
    }
}