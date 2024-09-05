using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using RoR2.Editor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.IMGUI.Controls;

namespace RoR2.Editor
{
    internal sealed class R2EKEditorProjectSettingsProvider : SettingsProvider
    {
        private R2EKEditorProjectSettings settings;
        private SerializedObject serializedObject;
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "RoR2EditorKit", "R2EK" };
            VisualElementTemplateDictionary.instance.DoSave();
            var settings = R2EKEditorProjectSettings.instance;
            if (R2EKSettings.instance.purgeDefaultProviders)
                EditorSettingManager.PurgeOrphanedSettings(settings);
            settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            settings.SaveSettings();
            return new R2EKEditorProjectSettingsProvider("Project/R2EK Editor Settings", SettingsScope.Project, keywords)
            {
                settings = settings,
                serializedObject = new SerializedObject(settings),
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            rootElement.Add(new EditorSettingsElement((EditorSettingManager.IEditorSettingProvider)settings));
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Save();
        }

        private void Save()
        {
        }
        public R2EKEditorProjectSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    }

}