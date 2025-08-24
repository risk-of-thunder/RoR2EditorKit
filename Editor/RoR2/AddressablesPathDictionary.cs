using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;
using UnityEngine.AddressableAssets;
using System.IO;
using HG;
using SimpleJSON;
using IOPath = System.IO.Path;

namespace RoR2.Editor
{
    [Serializable]
    public struct AddressablesPathDictionary
    {
        private const string PATH_TO_JSON = "Assets/" + FILE_NAME + ".json";
        private const string FILE_NAME = "lrapi_returns";
        public static AddressablesPathDictionary GetInstance()
        {
            if (!_instance.IsEmpty())
            {
                return _instance;
            }

            if(!System.IO.File.Exists(PATH_TO_JSON))
            {
                if(!EditorUtility.DisplayDialog("lrapi_returns.json missing.", "Could not find the lrapi_returns.json file in your project. This file should be located in your Assets folder.\n\nIf this file doesnt exist, click \"Ok\" to open a file picker from which the file will be obtained, the file is located under your \"[GAMEROOT]/Risk of Rain 2_Data/StreamingAssets\"", "Ok", "Cancel"))
                {
                    return default;
                }

                string directory = "Assets";
            openFilePanel:
                var path = EditorUtility.OpenFilePanel("Select lrapi_returns file", directory, "json");
                directory = Directory.GetParent(path).FullName;

                if (IOPath.GetFileNameWithoutExtension(path) != FILE_NAME)
                {
                    if(!EditorUtility.DisplayDialog("Invalid file", "The file is not the lrapi_returns.json file", "Try Again", "Cancel Operation"))
                        return default;

                    goto openFilePanel;
                }

                File.Copy(path, PATH_TO_JSON);
                AssetDatabase.ImportAsset(PATH_TO_JSON);
                AssetDatabase.SaveAssets();
            }

            var stopwatch = Stopwatch.StartNew();
            var jsonFile = System.IO.File.ReadAllText("Assets/lrapi_returns.json");

            var JSONNode = JSON.Parse(jsonFile);

            var regex = new Regex("Wwise");

            var pathToGUID = new Dictionary<string, string>(
                from key1 in JSONNode.Keys
                where !regex.Match(key1).Success
                select new KeyValuePair<string, string>(key1, JSONNode[key1].Value));

            var guidToPath = pathToGUID.ToDictionary(k => k.Value, k => k.Key);

            _instance = new AddressablesPathDictionary(pathToGUID, guidToPath);
            stopwatch.Stop();
            UnityEngine.Debug.Log($"AddressablesPathDictionary took " + stopwatch.ElapsedMilliseconds + "ms");
            return _instance;
        }

        private static AddressablesPathDictionary _instance;

        private Dictionary<string, string> pathToGUIDDictionary;
        private Dictionary<string, string> guidToPathDictionary;
        private string[] paths;
        private string[] guids;

        public bool IsEmpty()
        {
            return pathToGUIDDictionary == null || pathToGUIDDictionary.Count == 0;
        }

        public ReadOnlyArray<string> GetAllKeys()
        {
            return new ReadOnlyArray<string>(paths);
        }

        public ReadOnlyArray<string> GetAllKeysOfType(Type t)
        {
            if (t == null)
                return GetAllKeys();

            List<string> entries = new();
            foreach(var key in paths)
            {
                var resourceLocation = Addressables.LoadResourceLocationsAsync(key).WaitForCompletion().FirstOrDefault();
                var resourceType = resourceLocation?.ResourceType;

                if(resourceType != null && (resourceType == t || resourceType.IsSubclassOf(t)))
                {
                    entries.Add(key);
                }
            }
            return new ReadOnlyArray<string>(entries.ToArray());
        }

        public ReadOnlyArray<string> GetAllGUIDS()
        {
            return new ReadOnlyArray<string>(guids);
        }

        public ReadOnlyArray<string> GetAllGUIDSOfType(Type t)
        {
            if (t == null)
                return GetAllGUIDS();

            List<string> entries = new();
            foreach (var key in guids)
            {
                var resourceLocation = Addressables.LoadResourceLocationsAsync(key).WaitForCompletion().FirstOrDefault();
                var resourceType = resourceLocation?.ResourceType;

                if (resourceType != null && (resourceType == t || resourceType.IsSubclassOf(t)))
                {
                    entries.Add(key);
                }
            }
            return new ReadOnlyArray<string>(entries.ToArray());
        }

        public bool TryGetGUIDFromPath(string path, out string? guid)
        {
            return pathToGUIDDictionary.TryGetValue(path, out guid);
        }

        public string GetGUIDFromPath(string path)
        {
            return pathToGUIDDictionary[path];
        }

        public bool TryGetPathFromGUID(string guid, out string? path)
        {
            return guidToPathDictionary.TryGetValue(guid, out path);
        }

        public string GetPathFromGUID(string guid)
        {
            return guidToPathDictionary[guid];
        }

        private AddressablesPathDictionary(Dictionary<string, string> pathToGUID, Dictionary<string, string> guidToPath)
        {
            pathToGUIDDictionary = pathToGUID;
            paths = pathToGUID.Keys.ToArray();

            guidToPathDictionary = guidToPath;
            guids = guidToPath.Keys.ToArray();;
        }
    }
}