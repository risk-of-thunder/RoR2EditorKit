namespace RoR2.Editor
{
    /// <summary>
    /// <inheritdoc cref="ObjectEditingEditorWindow{TObject}"/>
    /// 
    /// <para>This one allows you to use VisualElements to create your UI</para>
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="ObjectEditingEditorWindow{TObject}"/></typeparam>
    public abstract class VisualElementObjectEditingWindow<T> : ObjectEditingEditorWindow<T> where T : UnityEngine.Object
    {
        protected sealed override void CreateGUI()
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, rootVisualElement, ValidatePath);
            FinalizeUI();
        }

        private void OnGUI() { }

        /// <summary>
        /// Finalize your UIElement's UI here. You can query the rootVisualElement to obtain your controls.
        /// </summary>
        protected abstract void FinalizeUI();

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making a window that isnt in the same assembly as R2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this editor window, false otherwise</returns>
        protected virtual bool ValidatePath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);
    }
}