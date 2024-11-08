using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    internal sealed class R2EKEditorPreferenceSettingsProvider : SettingsProvider
    {
        private R2EKEditorPreferenceSettings settings;
        private SerializedObject serializedObject;
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "RoR2EditorKit", "RoR2EK" };
            VisualElementTemplateDictionary.instance.DoSave();
            var settings = R2EKEditorPreferenceSettings.instance;

            settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            settings.SaveSettings();
            return new R2EKEditorPreferenceSettingsProvider("Preferences/RoR2EditorKit Editor Preferences", SettingsScope.User, keywords)
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
            settings.SaveSettings();
        }

        public R2EKEditorPreferenceSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    }

}