using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    [FilePath("ProjectSettings/RoR2EditorKit/R2EKSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class R2EKSettings : ScriptableSingleton<R2EKSettings>
    {
        public string tokenPrefix => _tokenPrefix;
        [SerializeField] private string _tokenPrefix;
        public bool tokenExists => !string.IsNullOrEmpty(_tokenPrefix) || !string.IsNullOrWhiteSpace(_tokenPrefix);
        public bool enableNamingConventions => _enableNamingConventions;
        [SerializeField] private bool _enableNamingConventions;
        public bool isFirstTimeBoot { get => _isFirstTimeBoot; set => _isFirstTimeBoot = value; }
        [SerializeField] private bool _isFirstTimeBoot = true;

        public bool enableThunderkitIntegration => _enableThunderkitIntegration;
        [SerializeField] private bool _enableThunderkitIntegration = false;

        [InitializeOnLoadMethod]
        public static void InitializeOnLoad()
        {
            EditorApplication.update += ShowSettingsWindow;
        }

        public string GetTokenAllUpperCase()
        {
            ThrowIfNoToken();
            return tokenPrefix.ToUpperInvariant();
        }

        public string GetTokenAllLowerCase()
        {
            ThrowIfNoToken();
            return tokenPrefix.ToLowerInvariant();
        }

        public string GetPrefixCamelCase()
        {
            ThrowIfNoToken();
            char[] array = tokenPrefix.ToCharArray();
            for(int i = 0; i < array.Length; i++)
            {
                array[i] = i == 0 ? char.ToUpperInvariant(array[i]) : char.ToLowerInvariant(array[i]);
            }

            return new string(array);
        }

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
                SettingsService.OpenProjectSettings("Project/RoR2EditorKit");
            }
        }

        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Main Settings")]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/RoR2EditorKit");
    }
}