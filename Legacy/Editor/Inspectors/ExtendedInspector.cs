using RoR2EditorKit.Data;
using RoR2EditorKit.VisualElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RoR2EditorKit.Inspectors
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;

    [Obsolete("Use the one inside the RoR2EditorKit namespace instead of this one. which works with the new ContextMenuHelper introduced in 4.0.0")]
    public struct ContextMenuData
    {
        public string menuName;
        public Action<DropdownMenuAction> menuAction;
        public Func<DropdownMenuAction, DropdownMenuAction.Status> actionStatusCheck;

        public ContextMenuData(string name, Action<DropdownMenuAction> action)
        {
            menuName = name;
            menuAction = action;
            actionStatusCheck = x => DropdownMenuAction.Status.Normal;
        }

        public ContextMenuData(string name, Action<DropdownMenuAction> action, Func<DropdownMenuAction, DropdownMenuAction.Status> statusCheck)
        {
            menuName = name;
            menuAction = action;
            actionStatusCheck = statusCheck;
        }
    }

    /// <summary>
    /// Base inspector for all the RoR2EditorKit Inspectors. Uses visual elements instead of IMGUI
    /// <para>Automatically retrieves the UXML asset for the editor by looking for an UXML asset with the same name as the inheriting type</para>
    /// <para>Extended Inspectors can be enabled or disabled</para>
    /// <para>If you want to make a Scriptable Object Inspector, you'll probably want to use the <see cref="ScriptableObjectInspector{T}"/></para>
    /// <para>If you want to make an Inspector for a Component, you'll probably want to use the <see cref="ComponentInspector{T}"/></para>
    /// </summary>
    /// <typeparam name="T">The type of Object being inspected</typeparam>
    public abstract class ExtendedInspector<T> : Editor where T : Object
    {
        #region Properties
        /// <summary>
        /// Access to the main RoR2EditorKit Settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings
        {
            get
            {
                if (!_settings)
                    _settings = RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>();
                return _settings;
            }
        }
        private static RoR2EditorKitSettings _settings;

        /// <summary>
        /// Retrieves the EditorSettings for this Inspector
        /// </summary>
        public EditorSetting EditorSettings
        {
            get
            {
                if(_editorSettings == null)
                {
                    _editorSettings = Settings.EditorSettings.GetOrCreateSettingsFor(GetType());
                }
                return _editorSettings;
            }
        }
        private EditorSetting _editorSettings;

        /// <summary>
        /// Check if the inspector is enabled
        /// <para>If you're setting the value, and the value is different from the old value, the inspector will redraw completely to accomodate the new look using either the base inspector or custom inspector</para>
        /// </summary>
        public bool InspectorEnabled
        {
            get
            {
                return EditorSettings.GetSetting("IsEditorEnabled", true);
            }
            set
            {
                var origValue = EditorSettings.GetSetting<bool>("IsEditorEnabled", true);
                if(origValue != value)
                {
                    EditorSettings.SetSetting("IsEditorEnabled", value);
                    OnInspectorEnabledChange();
                }
            }
        }

        /// <summary>
        /// The root visual element of the inspector, This is what gets returned by CreateInspectorGUI()
        /// <para>When the inspector is enabled, the "DrawInspectorElement" is added to this</para>
        /// <para>When the inspector is disabled, the "IMGUIContainerElement" with the default inspector is added to this.</para>
        /// </summary>
        protected VisualElement RootVisualElement
        {
            get
            {
                if (_rootVisualElement == null)
                {
                    _rootVisualElement = new VisualElement();
                    _rootVisualElement.name = "ExtendedInspector_RootElement";
                }

                return _rootVisualElement;
            }
        }
        private VisualElement _rootVisualElement;

        /// <summary>
        /// The root visual element where your custom inspector will be drawn.
        /// <para>This visual element will have the VisualTreeAsset applied.</para>
        /// </summary>
        protected VisualElement DrawInspectorElement
        {
            get
            {
                if (_drawInspectorElement == null)
                {
                    _drawInspectorElement = new VisualElement();
                    _drawInspectorElement.name = "ExtendedInspector_CustomEditor";
                }
                return _drawInspectorElement;
            }
        }
        private VisualElement _drawInspectorElement;

        /// <summary>
        /// The root visual element where the default, IMGUI inspector is drawn
        /// <para>This visual element will not have the VisualTreeAsset applied</para>
        /// <para>The IMGUIContainer that gets returned by the default inspector is added to this, it's name is "defaultInspector" if you need to Query it.</para>
        /// </summary>
        protected VisualElement IMGUIContainerElement
        {
            get
            {
                if (_imguiContianerElement == null)
                {
                    _imguiContianerElement = new VisualElement();
                    _imguiContianerElement.name = "ExtendedInspector_DefaultInspector";
                }
                return _imguiContianerElement;
            }
        }
        private VisualElement _imguiContianerElement;

        /// <summary>
        /// Wether the inspector has done its first drawing.
        /// <para>When the inspector draws for the first time, unity calls Bind() on <see cref="RootVisualElement"/>, this creates all the necesary fields for property fields, however, this runs only once.</para>
        /// <para>When HasDoneFirstDrawing is true, the ExtendedInspector will call Bind() to ensure property fields always appear.</para>
        /// </summary>
        protected bool HasDoneFirstDrawing { get => _hasDoneFirstDrawing; private set => _hasDoneFirstDrawing = value; }
        private bool _hasDoneFirstDrawing = false;

        /// <summary>
        /// Direct access to the object that's being inspected as its type.
        /// </summary>
        protected T TargetType { get => target as T; }

        /// <summary>
        /// If the editor has a visual tree asset, if set to false, RoR2EK will supress the null reference exception that appears from not having one.
        /// </summary>
        protected virtual bool HasVisualTreeAsset { get; } = true;
        #endregion Properties

        #region Fields
        private HelpBox namingConventionElement = null;
        #endregion Fields

        #region Methods
        /// <summary>
        /// Called when the inspector is enabled, always keep the original implementation unless you know what youre doing
        /// </summary>
        protected virtual void OnEnable()
        {
            EditorApplication.projectChanged += OnObjectNameChanged;
        }

        /// <summary>
        /// Called when the inspector is disabled, always keep the original implementation unless you know what you're doing
        /// </summary>
        protected virtual void OnDisable()
        {
            EditorApplication.projectChanged -= OnObjectNameChanged;

            if(serializedObject != null && serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnObjectNameChanged()
        {
            if (this == null || serializedObject == null || serializedObject.targetObject == null)
                return;

            if (serializedObject.targetObject && Settings.EditorSettings.enableNamingConventions && this is IObjectNameConvention objNameConvention)
            {
                PrefixData data = objNameConvention.GetPrefixData();

                bool flag = data.nameValidatorFunc == null ? serializedObject.targetObject.name.StartsWith(objNameConvention.Prefix) : data.nameValidatorFunc();
                if (flag)
                {
                    namingConventionElement?.RemoveFromHierarchy();
                    namingConventionElement = null;
                    return;
                }
                else if (namingConventionElement == null)
                {
                    namingConventionElement = EnsureNamingConventions(objNameConvention);
                    RootVisualElement.Add(namingConventionElement);
                    namingConventionElement.SendToBack();
                }
                else
                {
                    namingConventionElement.RemoveFromHierarchy();
                    RootVisualElement.Add(namingConventionElement);
                    namingConventionElement.SendToBack();
                }
            }
        }

        private void OnInspectorEnabledChange()
        {
            void ClearElements()
            {
                DrawInspectorElement.Wipe();
                IMGUIContainerElement.Wipe();
                RootVisualElement.Wipe();
            }

            ClearElements();
            OnRootElementsCleared?.Invoke();

            try
            {
                if (HasVisualTreeAsset)
                    GetTemplateInstance(GetType().Name, DrawInspectorElement, ValidateUXMLPath);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }

            OnVisualTreeCopy?.Invoke();

            OnObjectNameChanged();

            if (!InspectorEnabled)
            {
                var defaultImguiContainer = new IMGUIContainer(OnInspectorGUI);
                defaultImguiContainer.name = "defaultInspector";
                IMGUIContainerElement.Add(defaultImguiContainer);
                RootVisualElement.Add(IMGUIContainerElement);
                OnIMGUIContainerElementAdded?.Invoke();
            }
            else
            {
                DrawInspectorGUI();
                RootVisualElement.Add(DrawInspectorElement);
                OnDrawInspectorElementAdded?.Invoke();
                if (_hasDoneFirstDrawing)
                {
                    RootVisualElement.Bind(serializedObject);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making an inspector that isnt in the same assembly as RoR2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this inspector, false otherwise</returns>
        protected virtual bool ValidateUXMLPath(string path)
        {
            return VisualElementUtil.ValidateUXMLPath(path);
        }

        /// <summary>
        /// Cannot be overwritten, creates the inspector by checking if the editor is enabled or not
        /// <para>If the editor is enabled, the custom UI from the visual tree asset is drawn, to finish the implementation of said UI, implement <see cref="DrawInspectorGUI"/></para>
        /// <para>If the editor is disabled, the default IMGUI UI is drawn.</para>
        /// </summary>
        /// <returns></returns>
        public sealed override VisualElement CreateInspectorGUI()
        {
            OnInspectorEnabledChange();
            serializedObject.ApplyModifiedProperties();
            _hasDoneFirstDrawing = true;
            return RootVisualElement;
        }

        public sealed override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }

        private HelpBox EnsureNamingConventions(IObjectNameConvention objectNameConvention)
        {
            PrefixData prefixData = objectNameConvention.GetPrefixData();

            string message = prefixData.helpBoxMessage.IsNullOrEmptyOrWhitespace() ? $"This {typeof(T).Name}'s name should start with \"{objectNameConvention.Prefix} so it follows naming conventions" : prefixData.helpBoxMessage;
            HelpBox box = new HelpBox(message, MessageType.Info, true, CreateContextMenu);
            box.tooltip = prefixData.tooltipMessage;

            return box;

            void CreateContextMenu(ContextualMenuPopulateEvent evt)
            {
                evt.menu.AppendAction("Fix naming convention", (action) =>
                {
                    prefixData.contextMenuAction();
                    OnObjectNameChanged();
                });
            }
        }
        #endregion Methods

        #region Delegates
        /// <summary>
        /// Invoked when the RootVisualElement, DrawInspectorElement and IMGUIContainerElement are cleared;
        /// </summary>
        protected event Action OnRootElementsCleared;

        /// <summary>
        /// Invoked when the VisualTree assigned to this inspector has been copied to the "DrawInspectorElement"
        /// </summary>
        protected event Action OnVisualTreeCopy;

        /// <summary>
        /// Invoked right after "IMGUIContainerElement" is added to the "RootVisualElement"
        /// </summary>
        protected event Action OnIMGUIContainerElementAdded;

        /// <summary>
        /// Invoked right after the "DrawInspectorElement" is added to the "RootVisualElement"
        /// </summary>
        protected event Action OnDrawInspectorElementAdded;
        #endregion

        /// <summary>
        /// Implement The code functionality of your inspector here.
        /// </summary>
        protected abstract void DrawInspectorGUI();

        #region Util Methods

        [Obsolete("IMGUIContainer helpBoxes are no longer maintained, use the HelpBox VisualElement, or the CreateHelpBox(string, MessageType, bool, Action) method")]
        protected IMGUIContainer CreateHelpBox(string message, MessageType messageType)
        {
            IMGUIContainer container = new IMGUIContainer();
            container.name = $"ExtendedInspector_HelpBox";
            container.onGUIHandler = () =>
            {
                EditorGUILayout.HelpBox(message, messageType);
            };

            return container;
        }

        ///<summary>
        /// Creates a HelpBox
        /// </summary>
        /// <param name="message">The message that'll appear on the help box, either as straight text or as a tooltip</param>
        /// <param name="messageType">The severity of the message</param>
        /// <param name="isExplicit"> If true, the message is displayed with a label alongside an icon, if false, the text is stored and displayed as a tooltip when hovering over the HelpBox's Icon.</param>
        /// <param name="evt">An event to create a context menu for the help box.</param>
        /// <returns>A HelpBox</returns>
        protected HelpBox CreateHelpBox(string message, MessageType messageType, bool isExplicit, Action<ContextualMenuPopulateEvent> evt = null)
        {
            return new HelpBox(message, messageType, isExplicit, evt);
        }

        [Obsolete("This method has been made obsolete by the extension AddSimpleContextMenu that's implemented in ContextMenuHelper")]
        protected void AddSimpleContextMenu(VisualElement element, ContextMenuData contextMenuData)
        {
            element.AddSimpleContextMenu(new RoR2EditorKit.ContextMenuData(contextMenuData.menuName, contextMenuData.menuAction, contextMenuData.actionStatusCheck));
        }

        /// <summary>
        /// <inheritdoc cref="EditorSetting.GetSetting{T}(string, T)"/>
        /// </summary>
        protected T1 GetSetting<T1>(string settingName, T1 defaultValue = default)
        {
            return EditorSettings.GetSetting(settingName, defaultValue);
        }

        /// <summary>
        /// <inheritdoc cref="EditorSetting.SetSetting(string, object)"/>
        /// </summary>
        protected void SetSetting(string settingName, object value)
        {
            EditorSettings.SetSetting(settingName, value);
        }
        #endregion
    }
}
