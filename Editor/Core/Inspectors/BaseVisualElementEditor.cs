using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="BaseVisualElementEditor{T}"/> is a <see cref="BaseEditor{T}"/> built for creating UI using <see cref="VisualElement"/>s.
    /// <br></br>
    /// The <see cref="VisualTreeAsset"/> for the editor is obtained automatically, by finding an asset with it's name matching your Editor's TypeName.
    /// <br></br>
    /// You can ensure that the <see cref="VisualTreeAsset"/> is for your editor by overriding <see cref="ValidateVisualTreeAssetPath(string)"/>.
    /// <para></para>
    /// This editor doesnt support IMGUI, and usage of an <see cref="IMGUIContainer"/> may yield results of dubious quality with components. If you want to make an editor using IMGUI, utilize <see cref="BaseIMGUIEditor{T}"/> instead.
    /// <para></para>
    /// RoR2EK comes bundled with two <see cref="BaseVisualElementEditor{T}"/> types, which are the <see cref="ComponentVisualElementEditor{T}"/> and the <see cref="ScriptableObjectVisualElementEditor{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of Object being edited</typeparam>
    public abstract class BaseVisualElementEditor<T> : BaseEditor<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// The root visual element of this editor, custom controls should be added to this VisualElement, as this is returned by <see cref="CreateInspectorGUI"/>
        /// </summary>
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

        /// <summary>
        /// Checks if the inspector has done it's first drawing call
        /// </summary>
        protected bool hasDoneFirstDrawing { get; private set; }

        /// <summary>
        /// Event fired when <see cref="rootVisualElement"/> is cleared, this can be used to add elements that are displayed regardless if the inspector is enabled or disabled
        /// </summary>
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

        /// <summary>
        /// Base method that returns the VisualElement for this editor, you cannot override this.
        /// <br></br>
        /// Instead, implement <see cref="InitializeVisualElement"/>. The VisualElement for your editor is obtained by finding a <see cref="VisualTreeAsset"/> that matches your Editor's type name. Make sure to override <see cref="ValidateVisualTreeAssetPath(string)"/> to ensure your tree asset is picked.
        /// </summary>
        public sealed override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            ForceOnEditorEnabledChange();
            serializedObject.ApplyModifiedProperties();
            hasDoneFirstDrawing = true;
            return rootVisualElement;
        }

        /// <summary>
        /// Unavailable on VisualElement editors, if you wish to use IMGUI, utilize <see cref="BaseIMGUIEditor{T}"/> instead
        /// </summary>
        public sealed override void OnInspectorGUI() => base.OnInspectorGUI();

        private VisualElement CreateInspectorUI()
        {
            var visualElement = VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidateVisualTreeAssetPath);
            InitializeVisualElement(visualElement);
            return visualElement;
        }

        /// <summary>
        /// Utilize this method to initialize controls of your inspector, such as ensuring bindings, implementing context menus, etc.
        /// </summary>
        /// <param name="visualElement">The root of the template that was found for this inspector</param>
        protected virtual void InitializeVisualElement(VisualElement visualElement) { }

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making an inspector that isnt in the same assembly as R2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this inspector, false otherwise</returns>
        protected virtual bool ValidateVisualTreeAssetPath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);
    }
}