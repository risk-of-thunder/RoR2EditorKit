using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// base <see cref="Inspector{T}"/> that's used for creating Inspectors for <see cref="Component"/>s.
    /// <br>Implements a toggle on the UI that can be used to enable or disable the inspector</br>
    /// <para>See also <see cref="VisualElementComponentInspector{T}"/> and <see cref="IMGUIComponentInspector{T}"/></para>
    /// </summary>
    /// <typeparam name="T">The type of component being inspected</typeparam>
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