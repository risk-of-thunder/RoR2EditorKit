using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// A <see cref="ScriptableObjectInspector{T}"/> that allows you to implement the UI using VisualElements. It utilizes UXML Templates and finds the proper template by searching for a Template that has the same name as your inspector's class name.
    /// 
    /// <br>Ideal for Complex inspectors</br>
    /// </summary>
    /// <typeparam name="T">The type of ScriptableObject being inspected</typeparam>
    public abstract class VisualElementScriptableObjectInspector<T> : ScriptableObjectInspector<T> where T : ScriptableObject
    {
        /// <summary>
        /// <inheritdoc cref="Inspector{T}.CreateInspectorUI"/>
        /// <para>For <see cref="VisualElementScriptableObjectInspector{T}"/>, utilize <see cref="InitializeVisualElement"/> instead to initialize your visual element</para>
        /// </summary>
        protected sealed override VisualElement CreateInspectorUI()
        {
            var visualElement = VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidatePath);
            InitializeVisualElement(visualElement);
            return visualElement;
        }

        /// <summary>
        /// Utilize this method to initialize controls of your inspector, such as ensuring bindings, implementing context menus, etc.
        /// </summary>
        /// <param name="templateInstanceRoot">The root of the template that was found for this inspector</param>
        protected abstract void InitializeVisualElement(VisualElement templateInstanceRoot);

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making an inspector that isnt in the same assembly as R2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this inspector, false otherwise</returns>
        protected virtual bool ValidatePath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);
    }
}