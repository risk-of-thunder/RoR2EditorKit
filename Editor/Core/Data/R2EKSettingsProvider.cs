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
        private R2EKSettings _settings;
        private SerializedObject _serializedObject;
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "RoR2EditorKit" };
            VisualElementTemplateDictionary.instance.DoSave();
            var settings = R2EKSettings.instance;
            settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            settings.SaveSettings();
            return new R2EKSettingsProvider("Project/RoR2EditorKit/Settings", SettingsScope.Project, keywords)
            {
                _settings = settings,
                _serializedObject = new SerializedObject(settings),
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(R2EKSettings), rootElement);

            if(_settings.isFirstTimeBoot)
            {
                var center = rootElement.Q<VisualElement>("Center");
                var welcome = new ExtendedHelpBox("Thanks for downloading and using Risk of Rain 2 EditorKit. Inside this window you can see the settings to modify how RoR2EditorKit functions inside your project.\nRoR2EditorKit is a team effort by the community, mostly developed by Nebby1999 on discord. Consider donating to him for his dedication.\n\nThis window will only open once when installed, and this HelpBox will only show this time.", MessageType.Info, true, true);
                center.Add(welcome);
                welcome.SendToBack();
                _settings.isFirstTimeBoot = false;
                Save();
            }

            SetupNonFieldControls(rootElement);
            rootElement.Bind(_serializedObject);
        }

        private void SetupNonFieldControls(VisualElement root)
        {
            var button = root.Q<Button>("ImportIconButton");

            root.Q<Toggle>("EnableNamingConventions").SetEnabled(false);

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
            _serializedObject.ApplyModifiedProperties();
            Save();

            if(_settings.enableGameMaterialSystem && !ScriptingSymbolManager.ContainsDefine("R2EK_GAME_MATERIAL_SYSTEM"))
            {
                ScriptingSymbolManager.AddScriptingDefine("R2EK_GAME_MATERIAL_SYSTEM");
            }
            else if(!_settings.enableGameMaterialSystem && ScriptingSymbolManager.ContainsDefine("R2EK_GAME_MATERIAL_SYSTEM"))
            {
                ScriptingSymbolManager.RemoveScriptingDefine("R2EK_GAME_MATERIAL_SYSTEM");
            }
        }

        private void Save()
        {
            _serializedObject?.ApplyModifiedProperties();
            if (_settings)
                _settings.SaveSettings();
        }
        public R2EKSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    }

}