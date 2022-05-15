using RoR2EditorKit.Common;
using RoR2EditorKit.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace RoR2EditorKit.Core.EditorWindows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;

    /// <summary>
    /// Base window for creating an EditorWindow with visual elements.
    /// <para>An extended editor window can be used to createa more complex inspector for a type.</para>
    /// <para>The ExtendedEditorwindow contains both a WindowSerializedObject (created from the editor window itself) and a MainSerializedObject, which is used for the editing of a specific object.</para>
    /// </summary>
    /// <typeparam name="TObject">The type of object that's being edited on MainSerializedObject</typeparam>
    public abstract class ExtendedEditorWindow<TObject> : EditorWindow where TObject : Object
    {
        /// <summary>
        /// RoR2EK's main settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }
        /// <summary>
        /// The serialized object of this EditorWindow
        /// </summary>
        protected SerializedObject WindowSerializedObject { get; private set; }
        
        /// <summary>
        /// The Serialized object of the object being edited
        /// </summary>
        protected SerializedObject MainSerializedObject { get; private set; }

        /// <summary>
        /// Direct access to the MainSerializedObject's targetObject casted as <see cref="TObject"/>
        /// </summary>
        protected TObject TargetType { get; private set; }

        /// <summary>
        /// If true, when the ExtendedEditorWindow binds the RootVisualElement, it'll use the <see cref="MainSerializedObject"/>, otherwise it'll use the <see cref="WindowSerializedObject"/>
        /// </summary>
        protected virtual bool BindElementToMainSerializedObject { get; } = false;

        /// <summary>
        /// Opens the given editor window, and sets the MainSerializedObject
        /// </summary>
        /// <typeparam name="TEditorWindow">The type of window to open</typeparam>
        /// <param name="objectBeingEdited">The object that's going to be set to MainSerializedObject</param>
        /// <param name="windowName">Optional, the name of the window</param>
        public static void OpenEditorWindow<TEditorWindow>(Object objectBeingEdited, string windowName = null) where TEditorWindow : ExtendedEditorWindow<TObject>
        {
            TEditorWindow window = GetWindow<TEditorWindow>(windowName == null ? ObjectNames.NicifyVariableName(typeof(TEditorWindow).Name) : windowName);
            if(objectBeingEdited != null)
            {
                window.MainSerializedObject = new SerializedObject(objectBeingEdited);
                window.TargetType = window.MainSerializedObject.targetObject as TObject;
            }
            window.OnWindowOpened();
        }

        /// <summary>
        /// Finish any initialization here
        /// Keep base implementation unless you know what you're doing.
        /// <para>OnWindowOpened binds the root visual element to either the MainSerializedObject or WindowSerializedObject</para>
        /// <para>Execution order: OnEnable -> CreateGUI -> OnWindowOpened</para>
        /// </summary>
        protected virtual void OnWindowOpened()
        {
            rootVisualElement.Bind(BindElementToMainSerializedObject ? MainSerializedObject : WindowSerializedObject);
        }

        /// <summary>
        /// Called when the Editor Window is enabled, always keep the original implementation unless you know what youre doing
        /// Keep base implementation unless you know what you're doing.
        /// <para>OnEnable creates the WindowSerializedObject</para>
        /// <para>Execution order: OnEnable -> CreateGUI -> OnWindowOpened</para>
        /// </summary>
        protected virtual void OnEnable()
        {
            WindowSerializedObject = new SerializedObject(this);
        }

        /// <summary>
        /// Create or finalize your VisualElement UI here.
        /// Keep base implementation unless you know what you're doing.
        /// <para>RoR2EditorKit copies the VisualTreeAsset to the rootVisualElement in this method.</para>
        /// <para>Execution order: OnEnable -> CreateGUI -> OnWindowOpened</para>
        /// </summary>
        protected virtual void CreateGUI()
        {
            base.rootVisualElement.Clear();
            GetTemplateInstance(GetType().Name, rootVisualElement, ValidateUXMLPath);
        }

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making a window that isnt in the same assembly as RoR2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this editor window, false otherwise</returns>
        protected virtual bool ValidateUXMLPath(string path)
        {
            return path.StartsWith(Constants.AssetFolderPath) || path.StartsWith(Constants.AssetFolderPath);
        }
    }
}