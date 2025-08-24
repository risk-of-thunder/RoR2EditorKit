using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="R2EKEditorPreferenceSettings"/> is a <see cref="ScriptableSingleton{T}"/> that implements <see cref="EditorSettingManager.IEditorSettingProvider>"/>. It stores <see cref="EditorSettingCollection"/>s that are user preferences across all projects that use R2EK within the user's machine.
    /// 
    /// <para>Utilizing any of the methods within <see cref="EditorSettingManager"/> that accepts <see cref="EditorSettingManager.SettingType"/>, and using the <see cref="EditorSettingManager.SettingType.UserSetting"/> uses this provider.</para>
    /// <para>For specific project related settings, see <see cref="R2EKEditorProjectSettings"/></para>
    /// </summary>
    [FilePath("RoR2EditorKit/R2EKEditorPreferenceSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
    public sealed class R2EKEditorPreferenceSettings : ScriptableSingleton<R2EKEditorPreferenceSettings>, EditorSettingManager.IEditorSettingProvider
    {

        [SerializeField]
        private List<EditorSettingCollection> _projectSettings = new List<EditorSettingCollection>();

        List<EditorSettingCollection> EditorSettingManager.IEditorSettingProvider.editorSettings => _projectSettings;
        string EditorSettingManager.IEditorSettingProvider.providerName => nameof(R2EKEditorPreferenceSettings);
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
        /// Opens the UserSettings window and selects these settings.
        /// </summary>
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Preferences/Editor Settings")]
        public static void OpenSettings() => SettingsService.OpenUserPreferences("Preferences/RoR2EditorKit/User Editor Settings");

        internal sealed class R2EKEditorPreferenceSettingsProvider : SettingsProvider
        {
            private R2EKEditorPreferenceSettings settings;
            private SerializedObject serializedObject;
            [SettingsProvider]
            public static SettingsProvider CreateProvider()
            {
                var keywords = new[] { "RoR2EditorKit", "RoR2EK" };
                VisualElementTemplateDictionary.instance.DoSave();
                var settings = instance;

                settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                settings.SaveSettings();
                return new R2EKEditorPreferenceSettingsProvider("Preferences/RoR2EditorKit/Editor Settings", SettingsScope.User, keywords)
                {
                    settings = settings,
                    serializedObject = new SerializedObject(settings),
                };
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                rootElement.Add(new EditorSettingsElement(settings));
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
}
