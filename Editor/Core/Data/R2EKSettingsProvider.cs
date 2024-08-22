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
            base.OnActivate(searchContext, rootElement);
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(R2EKSettings), rootElement);

            if(settings.isFirstTimeBoot)
            {
                var center = rootElement.Q<VisualElement>("Center");
                var welcome = new HelpBox("Thanks for downloading and using Risk of Rain 2 EditorKit. Inside this window you can see the settings to modify how RoR2EditorKit functions inside your project.\nRoR2EditorKit is a team effort by the community, mostly developed by Nebby1999 on discord. Consider donating to him for his dedication.\n\nThis window will only open once when installed, and this HelpBox will only show this time.", MessageType.Info, true, true);
                center.Add(welcome);
                welcome.SendToBack();
                settings.isFirstTimeBoot = false;
                Save();
            }

            SetupNonFieldControls(rootElement);
        }

        private void SetupNonFieldControls(VisualElement root)
        {
            var button = root.Q<Button>("ImportIconButton");

            bool gizmosReadmeInProject = R2EKConstants.AssetGUIDs.gizmosReadme;
            button.SetEnabled(!gizmosReadmeInProject);

            button.clicked += () =>
            {
                AssetDatabase.ImportPackage(AssetDatabase.GUIDToAssetPath(R2EKConstants.AssetGUIDs.ror2IconsForScriptsGUID), false);
                button.SetEnabled(R2EKConstants.AssetGUIDs.gizmosReadme);
            };
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