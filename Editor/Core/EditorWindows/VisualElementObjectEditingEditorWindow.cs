namespace RoR2.Editor
{
    public abstract class VisualElementObjectEditingWindow<T> : ObjectEditingEditorWindow<T> where T : UnityEngine.Object
    {
        protected sealed override void CreateGUI()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, rootVisualElement, ValidatePath);
        }

        private void OnGUI() { }

        protected virtual bool ValidatePath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);
    }
}