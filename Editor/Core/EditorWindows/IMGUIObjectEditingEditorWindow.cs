using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// <inheritdoc cref="ObjectEditingEditorWindow{TObject}"/>
    /// 
    /// <para>This one allows you to use IMGUI to create the UI for your window.</para>
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="ObjectEditingEditorWindow{TObject}"/></typeparam>
    public abstract class IMGUIObjectEditingEditorWindow<TObject> : ObjectEditingEditorWindow<TObject> where TObject : UnityEngine.Object
    {
        protected sealed override void CreateGUI()
        {
            rootVisualElement.Add(new IMGUIContainer(OnGUI));
        }

        /// <summary>
        /// Implement your window's UI here.
        /// </summary>
        protected abstract void OnGUI();
    }
}