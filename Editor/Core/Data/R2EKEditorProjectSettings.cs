using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="R2EKEditorProjectSettings"/> is a <see cref="ScriptableSingleton{T}"/> that implements <see cref="EditorSettingManager.IEditorSettingProvider"/>. It stores <see cref="EditorSettingCollection"/>s that are used for this specific project.
    /// 
    /// <para>Utilizing any of the methods within <see cref="EditorSettingManager"/> that accepts <see cref="EditorSettingManager.SettingType"/>, and using the <see cref="EditorSettingManager.SettingType.ProjectSetting"/> uses this provider.</para>
    /// <para>For user specific related settings, see <see cref="R2EKEditorPreferenceSettings"/></para>
    /// </summary>
    [FilePath("ProjectSettings/RoR2EditorKit/R2EKEditorProjectSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class R2EKEditorProjectSettings : ScriptableSingleton<R2EKEditorProjectSettings>, EditorSettingManager.IEditorSettingProvider
    {
        [SerializeField]
        private List<EditorSettingCollection> _projectSettings = new List<EditorSettingCollection>();

        List<EditorSettingCollection> EditorSettingManager.IEditorSettingProvider.editorSettings => _projectSettings;
        string EditorSettingManager.IEditorSettingProvider.providerName => nameof(R2EKEditorProjectSettings);
        public void SaveSettings() => Save(true);

        private void Awake()
        {
            EditorApplication.quitting += EditorApplication_quitting;
        }

        private void EditorApplication_quitting()
        {
            Save(true);
        }

        private void OnDestroy()
        {
            EditorApplication.quitting -= EditorApplication_quitting;
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorSettingManager.RegisterProvider(instance, () => instance);
        }

        /// <summary>
        /// Opens the ProjectSettings window and selects these settings.
        /// </summary>
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Project/Editor Settings")]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/RoR2EditorKit/Editor Settings");

        internal sealed class R2EKEditorProjectSettingsProvider : SettingsProvider
        {
            private R2EKEditorProjectSettings settings;
            private SerializedObject serializedObject;
            [SettingsProvider]
            public static SettingsProvider CreateProvider()
            {
                var keywords = new[] { "RoR2EditorKit", "R2EK" };
                VisualElementTemplateDictionary.instance.DoSave();
                var settings = instance;
                if (R2EKSettings.instance.purgeProjectSettings)
                    EditorSettingManager.PurgeOrphanedSettings(settings);
                settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                settings.SaveSettings();
                return new R2EKEditorProjectSettingsProvider("Project/RoR2EditorKit/Editor Settings", SettingsScope.Project, keywords)
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
}
