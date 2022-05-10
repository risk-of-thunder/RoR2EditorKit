using RoR2EditorKit.Common;
using RoR2EditorKit.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.EditorWindows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;

    /// <summary>
    /// Base window for creating an EditorWindow with visual elements.
    /// </summary>
    public abstract class ExtendedEditorWindow : EditorWindow
    {
        /// <summary>
        /// RoR2EK's main settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings { get => RoR2EditorKitSettings.GetOrCreateSettings<RoR2EditorKitSettings>(); }
        /// <summary>
        /// The serialized object of this EditorWindow
        /// </summary>
        protected SerializedObject SerializedObject { get; private set; }

        /// <summary>
        /// Called when the Editor Window is enabled, always keep the original implementation unless you know what youre doing
        /// </summary>
        protected virtual void OnEnable()
        {
            base.rootVisualElement.Clear();
            GetTemplateInstance(GetType().Name, rootVisualElement, ValidateUXMLPath);
            SerializedObject = new SerializedObject(this);
            rootVisualElement.Bind(SerializedObject);
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

        private void CreateGUI()
        {
            DrawGUI();
        }

        /// <summary>
        /// Create or finalize your VisualElement UI here.
        /// </summary>
        protected abstract void DrawGUI();
    }
}