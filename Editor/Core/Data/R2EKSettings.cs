using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="R2EKSettings"/> is a <see cref="ScriptableSingleton{T}"/> and ProjectSetting that contains valuable information and configuration of RoR2EK for this specific project.
    /// 
    /// <para>These settings should only be modified using the ProjectSettings window.</para>
    /// </summary>
    [FilePath("ProjectSettings/RoR2EditorKit/R2EKSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class R2EKSettings : ScriptableSingleton<R2EKSettings>
    {
        /// <summary>
        /// The TokenPrefix to use for this project, case insensitive but should not contain underscores, dashes or spaces.
        /// </summary>
        public string tokenPrefix => _tokenPrefix;
        [SerializeField] private string _tokenPrefix;

        /// <summary>
        /// Checks if <see cref="tokenPrefix"/> has a valid value.
        /// </summary>
        public bool tokenExists => !string.IsNullOrEmpty(_tokenPrefix) || !string.IsNullOrWhiteSpace(_tokenPrefix);

        /// <summary>
        /// Checks if NamingConventions are Enabled in this project
        /// </summary>
        public bool enableNamingConventions => _enableNamingConventions;
        [SerializeField] private bool _enableNamingConventions = false;

        [Obsolete("Use \"hasHelpBoxBeenDismissed\" instead")]
        public bool isFirstTimeBoot { get => _hasHelpBoxBeenDismissed; set { } }

        /// <summary>
        /// Checks if the help box when ror2ek is first installed has been dismissed. This is used to make the project settings window open every time a domain reload is executed until the help box is dismissed.
        /// </summary>
        public bool hasHelpBoxBeenDismissed { get => _hasHelpBoxBeenDismissed; internal set => _hasHelpBoxBeenDismissed = value; }
        [SerializeField] private bool _hasHelpBoxBeenDismissed = false;

        /// <summary>
        /// Checks if R2EK should purge <see cref="R2EKEditorProjectSettings"/> of orphaned settings
        /// </summary>
        public bool purgeProjectSettings => _purgeProjectSettings;
        [SerializeField]
        private bool _purgeProjectSettings = false;

        /*public bool enableGameMaterialSystem => _enableGameMaterialSystem;
        [SerializeField] private bool _enableGameMaterialSystem;*/

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            EditorApplication.update += ShowSettingsWindow;
        }

        /// <summary>
        /// Retrieves the <see cref="tokenPrefix"/>, but with all characters as Upper Case
        /// </summary>
        /// <returns>The token prefix on Upper Case</returns>
        public string GetTokenAllUpperCase()
        {
            ThrowIfNoToken();
            return tokenPrefix.ToUpperInvariant();
        }

        /// <summary>
        /// Retrieves the <see cref="tokenPrefix"/>, but with all charactes as lower case
        /// </summary>
        /// <returns>The token prefix on lower case</returns>
        public string GetTokenAllLowerCase()
        {
            ThrowIfNoToken();
            return tokenPrefix.ToLowerInvariant();
        }

        /// <summary>
        /// Retrieves the <see cref="tokenPrefix"/>, but as camelCase
        /// </summary>
        /// <returns>The token prefix in a camelCase format</returns>
        public string GetTokenCamelCase()
        {
            ThrowIfNoToken();
            char[] array = tokenPrefix.ToCharArray();
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = i == 0 ? char.ToUpperInvariant(array[i]) : char.ToLowerInvariant(array[i]);
            }

            return new string(array);
        }
        [Obsolete("This method is wrongfuly named, use \"GetTokenCamelCase\" instead.")]
        public string GetPrefixCamelCase() => GetTokenCamelCase();

        /// <summary>
        /// Saves any modifications to the disk
        /// </summary>
        public void SaveSettings()
        {
            Save(true);
        }

        private void ThrowIfNoToken()
        {
            if (!tokenExists)
            {
                throw new NullReferenceException("Token Prefix is Null, Empty or Whitespace.");
            }
        }


        private void Awake()
        {
            EditorApplication.quitting += EditorApplication_quitting;
        }

        private void EditorApplication_quitting()
        {
            Save(true);
        }

        private static void ShowSettingsWindow()
        {
            EditorApplication.update -= ShowSettingsWindow;
            if (!instance.hasHelpBoxBeenDismissed)
            {
                SettingsService.OpenProjectSettings("Project/RoR2EditorKit/Settings");
            }
        }

        /// <summary>
        /// Opens the ProjectSettings window and selects these settings
        /// </summary>
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Project/Main Settings")]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/RoR2EditorKit/Main Settings");

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
                return new R2EKSettingsProvider("Project/RoR2EditorKit/Main Settings", SettingsScope.Project, keywords)
                {
                    _settings = settings,
                    _serializedObject = new SerializedObject(settings),
                };
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                base.OnActivate(searchContext, rootElement);
                VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(R2EKSettings), rootElement);

                if (!_settings.hasHelpBoxBeenDismissed)
                {
                    var center = rootElement.Q<VisualElement>("Center");
                    var welcome = new ExtendedHelpBox("Thanks for downloading and using Risk of Rain 2 EditorKit. Inside this window you can see the settings to modify how RoR2EditorKit functions inside your project.\r\nRoR2EditorKit is a team effort by the community, mostly developed by Nebby1999 on discord. Consider donating to him for his dedication.\r\n\r\nThis window will open every domain reload until this help-box is dismissed.", MessageType.Info, true, true);
                    welcome.dismissButton.clicked += () =>
                    {
                        _settings.hasHelpBoxBeenDismissed = true;
                        Save();
                    };
                    center.Add(welcome);
                    welcome.SendToBack();
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
                Save();
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
}