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
    internal sealed class R2EKSettingsProvider : SettingsProvider
    {
        private R2EKSettings settings;
        private SerializedObject serializedObject;
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "RoR2EditorKit" };
            VisualElementTemplateDictionary.instance.DoSave();
            var settings = R2EKSettings.instance;
            settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            settings.SaveSettings();
            return new R2EKSettingsProvider("Project/RoR2EditorKit", SettingsScope.Project, keywords)
            {
                settings = settings,
                serializedObject = new SerializedObject(settings),
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(R2EKSettings), rootElement);
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();
            Save();
        }

        private void Save()
        {
            serializedObject?.ApplyModifiedProperties();
            if (settings)
                settings.SaveSettings();
        }
        public R2EKSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    }

}