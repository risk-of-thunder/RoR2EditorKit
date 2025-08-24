using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using IOPath = System.IO.Path;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="R2EKPreferences"/> is a <see cref="ScriptableSingleton{T}"/> and PreferenceSetting that contains information and configuration of RoR2EK for this machine across all of it's projects.
    /// <para></para>
    /// These settings should only be modified using the PreferenceSettings window.
    /// </summary>
    [FilePath("RoR2EditorKit/R2EKPreferences.asset", FilePathAttribute.Location.PreferencesFolder)]
    public sealed class R2EKPreferences : ScriptableSingleton<R2EKPreferences>
    {
        private const string EXECUTABLE_NAME = "Risk of Rain 2.exe";
        const string STEAM_REGISTRY_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam";
        const uint ROR2_APP_ID = 632360U;

        /// <summary>
        /// Retrieves the path to the Risk of Rain 2 Executable.
        /// </summary>
        /// <returns>The executable's path, otherwise an empty string.</returns>
        public string GetGameExecutablePath()
        {
            if(string.IsNullOrWhiteSpace(_gameExecPath))
            {
                if(TryFindAppInstallDirectory(ROR2_APP_ID, out string directory))
                {
                    _gameExecPath = Path.Combine(directory, EXECUTABLE_NAME);
                    return _gameExecPath;
                }

                if(!EditorUtility.DisplayDialog("No Game Executable Found", "The risk of rain 2 Executable was not found automatically. Please select the Risk of Rain 2 Executable", "Open file picker", "Cancel Operation"))
                {
                    return "";
                }

                var path = EditorUtility.OpenFilePanel("Select the Risk of Rain 2 Executable", "Assets", "exe");
                var fileNameWithExtension = Path.GetFileName(path);
                if (fileNameWithExtension != EXECUTABLE_NAME)
                {
                    EditorUtility.DisplayDialog("Invalid Executable", "The executable you've selected is not Risk of Rain 2, please select the game's executable.", "Ok");
                    return "";
                }

                _gameExecPath = path;
                SaveSettings();
            }

            return _gameExecPath;
        }

        [SerializeField, FilePickerPath(FilePickerPath.PickerType.OpenFile, defaultName = "Risk of Rain 2", extension = "exe", title = "Select the Risk of Rain 2 Executable.")] 
        private string _gameExecPath;

        /// <summary>
        /// Saves any modifications to the disk
        /// </summary>
        public void SaveSettings()
        {
            Save(true);
        }

        private void Awake()
        {
            EditorApplication.quitting += EditorApplication_quitting;
        }

        private void EditorApplication_quitting()
        {
            Save(true);
        }

        /// <summary>
        /// Opens the PreferenceSettings window and selects these settings
        /// </summary>
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Preferences/Main Settings")]
        public static void OpenSettings() => SettingsService.OpenUserPreferences("Preferences/RoR2EditorKit/Main Settings");

        static bool TryFindAppSteamDirectory(uint appId, out string appSteamDirectory)
        {
            string steamInstallPath = Registry.GetValue(STEAM_REGISTRY_PATH, "InstallPath", null)?.ToString();
            if (string.IsNullOrEmpty(steamInstallPath))
            {
                appSteamDirectory = null;
                return false;
            }

            string libraryFoldersPath = Path.Combine(steamInstallPath, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFoldersPath))
            {
                appSteamDirectory = null;
                return false;
            }

            string currentPath = null;
            foreach (string line in File.ReadAllLines(libraryFoldersPath))
            {
                Match pathMatch = Regex.Match(line, "\"path\"\\s+\"(.+?)\"");
                if (pathMatch.Success)
                {
                    currentPath = pathMatch.Groups[1].Value;
                    continue;
                }

                Match appEntryMatch = Regex.Match(line, $"\"{appId}\"\\s+\"\\d+?\"");
                if (appEntryMatch.Success)
                {
                    if (currentPath != null)
                    {
                        appSteamDirectory = currentPath;
                        return true;
                    }
                }
            }

            appSteamDirectory = null;
            return false;
        }

        static bool TryFindAppInstallDirectory(uint appId, out string appInstallDirectory)
        {
            if (!TryFindAppSteamDirectory(appId, out string appSteamDirectory))
            {
                appInstallDirectory = null;
                return false;
            }

            string appManifestPath = Path.Combine(appSteamDirectory, "steamapps", $"appmanifest_{appId}.acf");
            if (!File.Exists(appManifestPath))
            {
                Console.WriteLine($"Failed to find app manifest at '{appManifestPath}'");

                appInstallDirectory = null;
                return false;
            }

            string appManifest = File.ReadAllText(appManifestPath);

            Match installDirMatch = Regex.Match(appManifest, "\"installdir\"\\s+\"(.+?)\"");
            if (!installDirMatch.Success)
            {
                Console.WriteLine($"Failed to find installdir in manifest '{appManifestPath}'");

                appInstallDirectory = null;
                return false;
            }

            string installDir = installDirMatch.Groups[1].Value;
            appInstallDirectory = Path.Combine(appSteamDirectory, "steamapps", "common", installDir);
            return true;
        }

        internal sealed class R2EKPreferencesProvider : SettingsProvider
        {
            private R2EKPreferences _settings;
            private SerializedObject _serializedObject;

            [SettingsProvider]
            public static SettingsProvider CreateProvider()
            {
                var keywords = new[] { "RoR2EditorKit", "Preferences" };
                VisualElementTemplateDictionary.instance.DoSave();
                var settings = instance;
                settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                settings.SaveSettings();
                return new R2EKPreferencesProvider("Preferences/RoR2EditorKit/Main Settings", SettingsScope.User, keywords)
                {
                    _settings = settings,
                    _serializedObject = new SerializedObject(settings)
                };
            }

            public override void OnActivate(string searchContext, VisualElement rootElement)
            {
                base.OnActivate(searchContext, rootElement);
                VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(R2EKPreferences), rootElement);

                var execPath = rootElement.Q<PropertyField>("GameExecutablePath");
                execPath.RegisterValueChangeCallback(OnGameExecPathSet);
                rootElement.Bind(_serializedObject);
            }

            private void OnGameExecPathSet(SerializedPropertyChangeEvent evt)
            {
                var changedProperty = evt.changedProperty;
                var path = changedProperty.stringValue;

                if(string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                var fileNameWithExtension = IOPath.GetFileName(path);
                if(fileNameWithExtension != EXECUTABLE_NAME)
                {
                    EditorUtility.DisplayDialog("Invalid Executable", "The executable you've selected is not Risk of Rain 2, please select the game's executable.", "Ok");
                    changedProperty.stringValue = "";
                    changedProperty.serializedObject.ApplyModifiedProperties();
                }
            }

            public override void OnDeactivate()
            {
                base.OnDeactivate();
                Save();
            }

            private void Save()
            {
                _serializedObject.ApplyModifiedProperties();
                if (_settings)
                    _settings.SaveSettings();
            }

            public R2EKPreferencesProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
            {
            }
        }
    }
}
