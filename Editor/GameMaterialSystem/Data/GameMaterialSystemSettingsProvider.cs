using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.GameMaterialSystem
{
    internal sealed class GameMaterialSystemSettingsProvider : SettingsProvider
    {
        private GameMaterialSystemSettings _settings;
        private SerializedObject _serializedObject;

        private ListView _listView;
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            var keywords = new[] { "GameMaterialSystem" };
            VisualElementTemplateDictionary.instance.DoSave();
            var settings = GameMaterialSystemSettings.instance;
            settings.hideFlags = UnityEngine.HideFlags.DontSave | UnityEngine.HideFlags.HideInHierarchy;
            settings.SaveSettings();

            return new GameMaterialSystemSettingsProvider("Project/RoR2EditorKit/Game Material System", SettingsScope.Project, keywords)
            {
                _settings = settings,
                _serializedObject = new SerializedObject(settings)
            };
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(GameMaterialSystemSettings), rootElement);

            _listView = rootElement.Q<ListView>();
            SetupNonFieldControls(rootElement);
            rootElement.Bind(_serializedObject);
        }

        private void SetupNonFieldControls(VisualElement root)
        {
            var saveButton = root.Q<Button>("SaveSettings");
            saveButton.clicked += Save;

            var autoPopulateButton = root.Q<Button>("AutoPopulate");
            autoPopulateButton.clicked += AutoPopulate;
        }

        private void AutoPopulate()
        {
            var stubbedShaders = AssetDatabaseUtil.FindAssetsByType<Shader>()
                .Where(s => s.name.StartsWith("Stubbed"))
                .ToList();

            bool anyAdded = false;
            foreach(var shader in stubbedShaders)
            {
                var newVal = _settings.AddShader(shader, false);
                if (newVal)
                    anyAdded = true;
            }

            if(anyAdded)
            {
                _settings.ReloadDictionary();
                Save();
                _listView.Rebuild();
            }
        }

        public override void OnDeactivate()
        {
            _serializedObject.ApplyModifiedProperties();
            Save();
        }

        private void Save()
        {
            _serializedObject?.ApplyModifiedProperties();
            if (_settings)
                _settings.SaveSettings();
        }


        public GameMaterialSystemSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }
    }
}