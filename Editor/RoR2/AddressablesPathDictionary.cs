using HG;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using IOPath = System.IO.Path;

namespace RoR2.Editor
{
    /// <summary>
    /// The AddressablesPathDictionary is a struct that contains the metadata stored within the game's "lrapi_returns.json", which is a Dictionary of Addressable Path to GUID.
    /// </summary>
    public struct AddressablesPathDictionary
    {
        private const string FILE_NAME = "lrapi_returns.json";

        private static string GetLrapiReturnsPath()
        {
            var gameExePath = R2EKPreferences.instance.GetGameExecutablePath();
            var directory = Directory.GetParent(gameExePath).FullName;
            var dataFolder = IOPath.Combine(directory, "Risk of Rain 2_Data");
            var streamingAssetsFolder = IOPath.Combine(dataFolder, "StreamingAssets");
            var filePath = IOPath.Combine(streamingAssetsFolder, FILE_NAME);
            return filePath;
        }

        /// <summary>
        /// Returns the instance currently stored.
        /// </summary>
        /// <returns>The instance of AddressablesPathDictionary</returns>
        public static AddressablesPathDictionary GetInstance()
        {
            if (!_instance.IsEmpty())
            {
                return _instance;
            }

            string lrapiReturnsPath = GetLrapiReturnsPath();
            if(!File.Exists(lrapiReturnsPath))
            {
                if(EditorUtility.DisplayDialog("LRAPI_RETURNS NOT FOUND", "The json file lrapi_returns was not found, This version of RoR2EditorKit requires the game to be at the very least post memory management update. The editor will now close.", "Ok"))
                {
                    EditorApplication.Exit(0);
                    return _instance;
                }
            }

            var stopwatch = Stopwatch.StartNew();
            var jsonFile = System.IO.File.ReadAllText(lrapiReturnsPath);

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

        /// <summary>
        /// Returns true if the AddressablesPathDictionary holds no data whatsoever.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return (pathToGUIDDictionary == null || pathToGUIDDictionary.Count == 0) || 
                (guidToPathDictionary == null || guidToPathDictionary.Count == 0) || 
                (paths == null || paths.Length == 0) ||
                (guids == null || guids.Length == 0);
        }

        /// <summary>
        /// Returns an array of all the Addressables Paths stored in the dictionary.
        /// </summary>
        public ReadOnlyArray<string> GetAllPaths()
        {
            return new ReadOnlyArray<string>(paths);
        }

        /// <summary>
        /// Returns an array of all the Addressables Paths stored in the dictionary, from which their assets inherit from <paramref name="t"/>
        /// <br></br>
        /// For example, GetAllPathsOfType(typeof(ItemDef)) will return all paths that represent ItemDefs.
        /// </summary>
        /// <param name="t">The type of asset</param>
        public ReadOnlyArray<string> GetAllPathsOfType(Type t)
        {
            return GetAllPathsOfTypes(new Type[] { t });
        }

        /// <summary>
        /// Returns an array of all the Addressables Paths stored in the dictionary, from which their assets inherit from any of the types specified in <paramref name="types"/>
        /// </summary>
        /// <br></br>
        /// For example, GetAllGUIDSOfType(new Type[] { typeof(ItemDef), typeof(EquipmentDef) }) will return all paths that represent either ItemDefs OR EquipmentDefs.
        /// <param name="types">The types of assets.</param>
        public ReadOnlyArray<string> GetAllPathsOfTypes(Type[] types)
        {
            if (types == null || types.Length == 0)
                return GetAllPaths();

            List<string> entries = new();
            foreach (var key in paths)
            {
                var resourceLocations = Addressables.LoadResourceLocationsAsync(key).WaitForCompletion();
                var resourceLocation = resourceLocations.FirstOrDefault();
                var resourceType = resourceLocation?.ResourceType;

                if (resourceType != null && (types.Contains(resourceType) || types.Any(t => resourceType.IsSubclassOf(t))))
                {
                    entries.Add(key);
                }
            }
            return new ReadOnlyArray<string>(entries.ToArray());
        }

        /// <summary>
        /// Returns an array of all the Addressables GUIDS stored in the dictionary.
        /// </summary>
        public ReadOnlyArray<string> GetAllGUIDS()
        {
            return new ReadOnlyArray<string>(guids);
        }

        /// <summary>
        /// Returns an array of all the Addressables GUIDS stored in the dictionary, from which their assets inherit from <paramref name="t"/>
        /// <br></br>
        /// For example, GetAllGUIDSOfType(typeof(ItemDef)) will return all GUIDS that represent ItemDefs.
        /// </summary>
        /// <param name="t">The type of asset</param>
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

        /// <summary>
        /// Returns an array of all the Addressables GUIDS stored in the dictionary, from which their assets inherit from any of the types specified in <paramref name="types"/>
        /// </summary>
        /// <br></br>
        /// For example, GetAllGUIDSOfType(new Type[] { typeof(ItemDef), typeof(EquipmentDef) }) will return all GUIDS that represent either ItemDefs OR EquipmentDefs.
        /// <param name="types">The types of assets.</param>
        public ReadOnlyArray<string> GetAllGUIDSOfTypes(Type[] types)
        {
            if (types == null || types.Length == 0)
                return GetAllGUIDS();

            List<string> entries = new();
            foreach (var key in guids)
            {
                var resourceLocation = Addressables.LoadResourceLocationsAsync(key).WaitForCompletion().FirstOrDefault();
                var resourceType = resourceLocation?.ResourceType;

                if (resourceType != null && (types.Contains(resourceType) || types.Any(t => resourceType.IsSubclassOf(t))))
                {
                    entries.Add(key);
                }
            }
            return new ReadOnlyArray<string>(entries.ToArray());
        }

        /// <summary>
        /// Tries to obtain a guid from it's addressable path.
        /// </summary>
        /// <param name="path">The addressable path, from which we want it's guid</param>
        /// <param name="guid">The resulting guid</param>
        /// <returns>True if the value was succesfuly obtained, otherwise false.</returns>
        public bool TryGetGUIDFromPath(string path, out string? guid)
        {
            return pathToGUIDDictionary.TryGetValue(path, out guid);
        }

        /// <summary>
        /// Obtains a guid from <paramref name="path"/> directly with no safety.
        /// </summary>
        /// <param name="path">The addressable path from which we want it's guid</param>
        /// <returns>The guid itself.</returns>
        public string GetGUIDFromPath(string path)
        {
            return pathToGUIDDictionary[path];
        }

        /// <summary>
        /// Tries to obtain a path from it's addressable guid.
        /// </summary>
        /// <param name="path">The resulting path</param>
        /// <param name="guid">The addressable guid, from which we want it's path</param>
        /// <returns>True if the value was succesfuly obtained, otherwise false.</returns>
        public bool TryGetPathFromGUID(string guid, out string? path)
        {
            return guidToPathDictionary.TryGetValue(guid, out path);
        }

        /// <summary>
        /// Obtains a path from <paramref name="guid"/> directly with no safety.
        /// </summary>
        /// <param name="guid">The addressable guid from which we want it's path</param>
        /// <returns>The path itself.</returns>
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