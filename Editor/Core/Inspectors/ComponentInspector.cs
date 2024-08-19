using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class ComponentInspector<T> : Inspector<T> where T : Component
    {
        private VisualElement toggleTemplateInstance;
        private Toggle _toggle;
        protected virtual void OnEnable()
        {
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