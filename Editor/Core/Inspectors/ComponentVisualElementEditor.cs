using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="ComponentVisualElementEditor{T}"/> is a <see cref="BaseVisualElementEditor{T}"/> built for inspector components.
    /// <br></br>
    /// A toggle for enabling or disabling the inspector appears at the top of the UI.
    /// <para></para>
    /// Ideal for complex editors.
    /// <para></para>
    /// It is not recommended to use <see cref="IMGUIContainer"/> to build your editor UI. If you want to make an editor for components with IMGUI, utilize <see cref="ComponentIMGUIEditor{T}"/> instead.
    /// </summary>
    /// <typeparam name="T">The type of component being edited</typeparam>
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