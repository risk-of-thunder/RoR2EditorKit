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

                if (string.IsNullOrWhiteSpace(path))
                {
                    return;
                }

                var fileNameWithExtension = IOPath.GetFileName(path);
                if (fileNameWithExtension != EXECUTABLE_NAME)
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
        private const string EXECUTABLE_NAME = "Risk of Rain 2.exe";
        private const string STEAM_REGISTRY_PATH = @"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam";
        private const string EXECUTABLE_OPERATION_CANCELLED_MESSAGE = "The operation for finding the Risk of Rain 2 Executable has been cancelled, the full capabilities of the Risk of Rain 2 Editor Kit may not be available.";
        const uint ROR2_APP_ID = 632360U;

        /// <summary>
        /// Retrieves the path to the Risk of Rain 2 Executable.
        /// </summary>
        /// <returns>The executable's path, otherwise an empty string.</returns>
        public string GetGameExecutablePath()
        {
            //Path does not exist, time to find it!
            if(string.IsNullOrWhiteSpace(_gameExecPath))
            {
                //Tried to find it with this method, we succeeded and the path is valid, save it and return.
                if(TryFindGameExecutable(ROR2_APP_ID, out var gameExePath) && IsGameExecPathValid(gameExePath))
                {
                    _gameExecPath = gameExePath;
                    SaveSettings();
                    return _gameExecPath;
                }

                //We vailed to find it manually, init dialog.
                if (!EditorUtility.DisplayDialog("Game Executable Missing", "The Risk of Rain 2 Executable in your preference settings has gone missing. Please select the Risk of Rain 2 Executable", "Open file picker", "Cancel Operation"))
                {
                    //Operation cancelled
                    DisplayOperationCancelledDialogAndThrowException();
                }

                //This method internally uses IsGameExecPathValid, so if it returns true we know we got a match.
                if (TryFindExecutableWithOpenFilePanel(out var selectedPath))
                {
                    _gameExecPath = selectedPath;
                    SaveSettings();
                    return _gameExecPath;
                }
            }

            //We have the path, validate it just in case.
            if(IsGameExecPathValid() == false)
            {
                //Bogus path, init dialog.
                if (!EditorUtility.DisplayDialog("Game Executable Missing", "The Risk of Rain 2 Executable in your preference settings has gone missing. Please select the Risk of Rain 2 Executable", "Open file picker", "Cancel Operation"))
                {
                    //Operation cancelled
                    DisplayOperationCancelledDialogAndThrowException();
                }

                //This method internally uses IsGameExecPathValid, so if it returns true we know we got a match.
                if (TryFindExecutableWithOpenFilePanel(out var selectedPath))
                {
                    _gameExecPath = selectedPath;
                    SaveSettings();
                    return _gameExecPath;
                }
            }

            //Path is not bogus
            return _gameExecPath;
        }

        private bool TryFindExecutableWithOpenFilePanel(out string selectedPath)
        {
        openFilePanel:
            var path = EditorUtility.OpenFilePanel("Select the Risk of Rain 2 Executable", "Assets", "exe");
            var fileNameWithExtension = Path.GetFileName(path);
            
            //File name does not match executable name, display dialog
            if(IsGameExecPathValid(path) == false)
            {
                if(!EditorUtility.DisplayDialog("Invalid Executable", "The Executable you've selected is not Risk of Rain 2, please select the game's executable.", "Ok", "Cancel Operation"))
                {
                    //Operation Cancelled
                    DisplayOperationCancelledDialogAndThrowException();
                }

                //Go to openFilePanel
                goto openFilePanel;
            }

            selectedPath = path;
            return true;
        }

        private bool IsGameExecPathValid(string gameExecPath = null)
        {
            gameExecPath ??= _gameExecPath;

            bool fileExists = File.Exists(gameExecPath);
            bool fileNameMatches = (Path.GetFileName(gameExecPath) == EXECUTABLE_NAME);

            return fileExists && fileNameMatches;
        }

        private void DisplayOperationCancelledDialogAndThrowException()
        {
            EditorUtility.DisplayDialog("Operation Cancelled", EXECUTABLE_OPERATION_CANCELLED_MESSAGE, "Ok");
            throw new OperationCanceledException("The operation for finding the Risk of Rain 2 Executable was cancelled.");
        }

        [SerializeField, FilePickerPath(FilePickerPath.PickerType.OpenFile, defaultName = "Risk of Rain 2", extension = "exe", title = "Select the Risk of Rain 2 Executable.")] 
        private string _gameExecPath;

        public string gameDirectoryPath => Path.GetDirectoryName(GetGameExecutablePath());
        public string gameDataPath => Path.Combine(gameDirectoryPath, $"{Path.GetFileNameWithoutExtension(GetGameExecutablePath())}_Data");
        public string managedAssembliesPath => Path.Combine(gameDataPath, $"Managed");
        public string streamingAssetsPath => Path.Combine(gameDataPath, "StreamingAssets");
        public string addressableAssetsPath => Path.Combine(streamingAssetsPath, "aa");
        public string addressableAssetsCatalog => Path.Combine(addressableAssetsPath, "catalog.json");
        public string addressableAssetsSettings => Path.Combine(addressableAssetsPath, "settings.json");

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

        static bool TryFindGameExecutable(uint appID, out string gameExecutable)
        {
            if(TryFindAppInstallDirectory(appID, out var gameDirectory))
            {
                gameExecutable = Path.Combine(gameDirectory, EXECUTABLE_NAME);
                return true;
            }
            gameExecutable = "";
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
    }
}
