using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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

        /// <summary>
        /// Checks if this is the first time R2EK has been booted, used to open the project settings window when the package is first installed and shows a special help box as a result.
        /// </summary>
        public bool isFirstTimeBoot { get => _isFirstTimeBoot; set => _isFirstTimeBoot = value; }
        [SerializeField] private bool _isFirstTimeBoot = true;

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
            if(!tokenExists)
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
            if(instance._isFirstTimeBoot)
            {
                Debug.Log("Showing ror2ek settings.");
                SettingsService.OpenProjectSettings("Project/RoR2EditorKit/Settings");
            }
        }

        /// <summary>
        /// Opens the ProjectSettings window and selects these settings
        /// </summary>
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Main Settings")]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/RoR2EditorKit/Settings");
    }
}