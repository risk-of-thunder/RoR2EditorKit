using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class BaseVisualElementEditor<T> : BaseEditor<T> where T : UnityEngine.Object
    {
        public VisualElement rootVisualElement
        {
            get
            {
                if(_rootVisualElement == null)
                {
                    _rootVisualElement = new VisualElement();
                    _rootVisualElement.name = $"{typeof(T).Name}_RootElement";
                }
                return _rootVisualElement;
            }
        }
        private VisualElement _rootVisualElement;

        protected bool hasDoneFirstDrawing { get; private set; }

        protected event Action onRootElementCleared;

        protected override void PreEditorEnabledCallbacks()
        {
            base.PreEditorEnabledCallbacks();
            rootVisualElement.Clear();
            onRootElementCleared?.Invoke();
        }

        protected override void OnEditorDisabled()
        {
            base.OnEditorDisabled();
            InspectorElement.FillDefaultInspector(rootVisualElement, serializedObject, this);
            rootVisualElement.Bind(serializedObject);
        }

        protected override void OnEditorEnabled()
        {
            base.OnEditorEnabled();
            var editorElement = CreateInspectorUI();
            rootVisualElement.Add(editorElement);
            if(hasDoneFirstDrawing)
            {
                rootVisualElement.Bind(serializedObject);
            }
        }

        public sealed override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            ForceOnEditorEnabledChange();
            serializedObject.ApplyModifiedProperties();
            hasDoneFirstDrawing = true;
            return rootVisualElement;
        }

        public sealed override void OnInspectorGUI() => base.OnInspectorGUI();

        private VisualElement CreateInspectorUI()
        {
            var visualElement = VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidateVisualTreeAssetPath);
            InitializeVisualElement(visualElement);
            return visualElement;
        }

        protected virtual void InitializeVisualElement(VisualElement visualElement) { }
        protected virtual bool ValidateVisualTreeAssetPath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);
    }
}