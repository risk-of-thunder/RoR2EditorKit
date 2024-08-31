using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class VisualElementScriptableObjectInspector<T> : ScriptableObjectInspector<T> where T : ScriptableObject
    {
        protected sealed override VisualElement CreateInspectorUI()
        {
            var visualElement = VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidatePath);
            InitializeVisualElement(visualElement);
            return visualElement;
        }

        protected abstract void InitializeVisualElement(VisualElement templateInstanceRoot);

        protected virtual bool ValidatePath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);
    }
}