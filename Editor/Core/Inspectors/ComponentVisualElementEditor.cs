using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class ComponentVisualElementEditor<T> : BaseVisualElementEditor<T> where T : Component
    {
        private VisualElement _toggleTemplateInstance;
        private Toggle _toggle;

        protected override void OnEnable()
        {
            base.OnEnable();
            _toggleTemplateInstance = VisualElementTemplateDictionary.instance.GetTemplateInstance("ComponentInspectorEnableToggle", null, (str) => str.Contains(R2EKConstants.PACKAGE_NAME));
            _toggle = _toggleTemplateInstance.Q<Toggle>();
            _toggle.value = editorEnabled;
            _toggle.RegisterValueChangedCallback(ce => editorEnabled = ce.newValue);
            onRootElementCleared += AddToggle;
        }

        private void AddToggle()
        {
            rootVisualElement.Add(_toggleTemplateInstance);
        }
    }
}