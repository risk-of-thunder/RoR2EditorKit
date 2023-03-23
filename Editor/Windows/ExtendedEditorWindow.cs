using RoR2EditorKit.Data;
using UnityEditor;
using UnityEditor.UIElements;

namespace RoR2EditorKit.EditorWindows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;

    /// <summary>
    /// Base EditorWindow for all the RoR2EditorKit Editor Windows. Uses VisualElements instead of IMGUI
    /// <para>Automatically retrieves the UXML asset for the editor by looking for an asset with the same name as the inheriting type</para>
    /// <para>If you want to create an EditorWindow for editing an object, you'll probably want to use the <see cref="ObjectEditingEditorWindow{TObject}"/></para>
    /// </summary>
    public abstract class ExtendedEditorWindow : EditorWindow
    {
        /// <summary>
        /// RoR2EK's main settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings { get => ThunderKit.Core.Data.ThunderKitSetting.GetOrCreateSettings<RoR2EditorKitSettings>(); }

        /// <summary>
        /// The serialized object for this window
        /// </summary>
        protected SerializedObject SerializedObject
        {
            get
            {
                return serializedObject;
            }
            set
            {
                if (serializedObject != value)
                {
                    serializedObject = value;
                    UpdateBinding();
                    return;
                }
                else
                {
                    serializedObject = value;
                }
            }
        }
        private SerializedObject serializedObject;

        /// <summary>
        /// Opens an ExtendedEditorWindow without setting a <see cref="SerializedObject"/> for the window.
        /// </summary>
        /// <typeparam name="TEditorWindow">The type of ExtendedEditorWindow to Open</typeparam>
        /// <returns>The instancianced window.</returns>
        public static TEditorWindow OpenEditorWindow<TEditorWindow>() where TEditorWindow : ExtendedEditorWindow
        {
            TEditorWindow window = GetWindow<TEditorWindow>(ObjectNames.NicifyVariableName(typeof(TEditorWindow).Name));
            window.OnWindowOpened();
            return window;
        }

        /// <summary>
        /// Opens an ExtendedEditorWindow without setting a <see cref="SerializedObject"/> for the window.
        /// </summary>
        /// <typeparam name="TEditorWindow">The type of ExtendedEditorWindow to Open</typeparam>
        /// <param name="serializedObjectForWindow">The SerializedObject for this window, leaving this null will create a new SerializedObject from this window.</param>
        /// <param name="windowName">The name for this window, leaving this null nicifies the <typeparamref name="TEditorWindow"/>'s type name</param>
        /// <returns>The instancianced window.</returns>
        public static TEditorWindow OpenEditorWindow<TEditorWindow>(SerializedObject serializedObjectForWindow = null, string windowName = null) where TEditorWindow : ExtendedEditorWindow
        {
            TEditorWindow window = GetWindow<TEditorWindow>(windowName ?? ObjectNames.NicifyVariableName(typeof(TEditorWindow).Name));
            window.SerializedObject = serializedObjectForWindow ?? new SerializedObject(window);
            window.OnWindowOpened();
            return window;
        }

        /// <summary>
        /// Finish any initialization here
        /// Keep base implementation unless you know what you're doing.
        /// <para>Execution order: OnEnable -> CreateGUI -> OnWindowOpened</para>
        /// </summary>
        protected virtual void OnWindowOpened() { }

        /// <summary>
        /// Create or finalize your VisualElement UI here.
        /// Keep base implementation unless you know what you're doing.
        /// <para>RoR2EditorKit copies the VisualTreeAsset to the rootVisualElement in this method.</para>
        /// <para>Execution order: OnEnable -> CreateGUI -> OnWindowOpened</para>
        /// </summary>
        protected virtual void CreateGUI()
        {
            rootVisualElement.Clear();
            GetTemplateInstance(GetType().Name, rootVisualElement, ValidateUXMLPath);
        }

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making a window that isnt in the same assembly as RoR2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this editor window, false otherwise</returns>
        protected virtual bool ValidateUXMLPath(string path)
        {
            return path.StartsWith(Constants.PackageFolderPath);
        }

        private void UpdateBinding()
        {
            rootVisualElement.Unbind();
            /*var pfs = rootVisualElement.Query<PropertyField>().Build();
            foreach (PropertyField field in pfs.ToList())
            {
                field.Clear();
            }*/
            rootVisualElement.Bind(serializedObject);
        }
    }
}