using HG;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using IOPath = System.IO.Path;
using Debug = UnityEngine.Debug;
using System.Text;

namespace RoR2.Editor
{
    /// <summary>
    /// The AddressablesPathDictionary is a struct that contains the metadata stored within the game's "lrapi_returns.json", which is a Dictionary of Addressable Path to GUID.
    /// </summary>
    [FilePath(PerUserProjectSettingHelper.ROOT_FILE_PATH + ADDRESSABLES_PATH_DICTIONARY_FILE_NAME, FilePathAttribute.Location.ProjectFolder)]
    public sealed class AddressablesPathDictionary : ScriptableSingleton<AddressablesPathDictionary>, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Represents an entry type within the dictionary
        /// </summary>
        public enum EntryType
        {
            /// <summary>
            /// Represents a GUID entry.
            /// </summary>
            Guid,

            /// <summary>
            /// Represents a Path string entry
            /// </summary>
            Path
        }

        /// <summary>
        /// Class utilized for looking up entries within the <see cref="AddressablesPathDictionary"/>.
        /// <para></para>
        /// This is an <see cref="IDisposable"/> class, be sure to call <see cref="Dispose"/> once you finish utilizing the lookup.
        /// </summary>
        public class EntryLookup : IDisposable
        {
            /// <summary>
            /// The type of entries we're looking up
            /// </summary>
            public EntryType entryLookupType = EntryType.Path;

            /// <summary>
            /// Only entires that are of these types are returned, types are treated as OR.
            /// </summary>
            public Type[] typeRestriction = Array.Empty<Type>();

            /// <summary>
            /// If the <see cref="typeRestriction"/> is of type <see cref="GameObject"/>, you can utilize this field to specify a component that the game object is required to have
            /// </summary>
            public Type componentRequirement = null;

            /// <summary>
            /// If true, the component search of <see cref="componentRequirement"/> will use <see cref="GameObject.GetComponentInChildren(Type)"/> instead of <see cref="GameObject.TryGetComponent(Type, out Component)"/>.
            /// </summary>
            public bool searchComponentInChildren = false;

            /// <summary>
            /// An optional filter, only entires that contain this string are returned
            /// </summary>
            public string filter;

            /// <summary>
            /// The results of the lookup, this is populated after <see cref="PerformLookup"/> or <see cref="PerformLookupAsync"/>
            /// </summary>
            public ReadOnlyCollection<string> results;
            public List<string> _results;

            /// <summary>
            /// Performs the Lookup with the given parameters
            /// </summary>
            /// <returns>The results of the lookup</returns>
            public ReadOnlyCollection<string> PerformLookup()
            {
                IEnumerator sync = PerformLookupAsync();
                while (sync.MoveNext())
                {
                }
                return results;
            }

            /// <summary>
            /// Performs the lookup with the given paramters in an async fashion
            /// </summary>
            /// <returns>A coroutine that can be awaited, once it's complete, the results are stored within <see cref="results"/></returns>
            public IEnumerator PerformLookupAsync()
            {
                _results = ListPool<string>.RentCollection();

                if(typeRestriction.Length == 0 && (componentRequirement != null && componentRequirement.IsSubclassOf(typeof(Component))))
                {
                    HG.ArrayUtils.ArrayAppend(ref typeRestriction, typeof(GameObject));
                }

                //Get each result for each type, request entries returns the cache if it exists, and if not, it builds it
                foreach (Type type in typeRestriction)
                {
                    yield return null;
                    string[] cache = instance.dictionaryCache.RequestEntries(type, componentRequirement, searchComponentInChildren, entryLookupType);


                    var modulo = Mathf.Floor((Mathf.Log10(cache.Length) + 1) * 2);
                    for (int i = 0; i < cache.Length; i++)
                    {
                        string cacheEntry = cache[i];

                        if (i % modulo == 0)
                        {
                            yield return null;
                        }

                        //If we have a filter, filter the results
                        if (!filter.IsNullOrEmptyOrWhiteSpace())
                        {
                            if (cacheEntry.Contains(filter) && !_results.Contains(cacheEntry))
                            {
                                _results.Add(cacheEntry);
                            }
                        }
                        else
                        {
                            if (!_results.Contains(cacheEntry))
                            {
                                _results.Add(cacheEntry);
                            }
                        }
                    }
                }

                //Store the results
                results = new ReadOnlyCollection<string>(_results);
                yield break;
            }

            /// <summary>
            /// Sets <see cref="entryLookupType"/>'s value to <paramref name="entryType"/>
            /// </summary>
            /// <param name="entryType">The entry type to utilize for the lookup</param>
            /// <returns>Itself</returns>
            public EntryLookup WithLookupType(EntryType entryType)
            {
                entryLookupType = entryType;
                return this;
            }

            /// <summary>
            /// Sets <see cref="typeRestriction"/>'s values to the types passed in <paramref name="types"/>
            /// </summary>
            /// <param name="types">The Types to utilize as a restriction</param>
            /// <returns>Itself</returns>
            public EntryLookup WithTypeRestriction(params Type[] types)
            {
                typeRestriction = types;
                return this;
            }

            public EntryLookup WithComponentRequirement(Type componentRequirement, bool searchComponentInChildren)
            {
                this.componentRequirement = componentRequirement;
                this.searchComponentInChildren = searchComponentInChildren;
                return this;
            }

            /// <summary>
            /// Sets <see cref="filter"/>'s values to the types passed in <paramref name="filter"/>
            /// </summary>
            /// <param name="filter">The filter to utilize</param>
            /// <returns>Itself</returns>
            public EntryLookup WithFilter(string filter)
            {
                this.filter = filter;
                return this;
            }

            /// <summary>
            /// Disposes the resources utilized for the lookup
            /// </summary>
            public void Dispose()
            {
                if(_results != null)
                {
                    ListPool<string>.ReturnCollection(_results);
                    _results = null;
                }
            }
        }

        private class Entry
        {
            public string path;
            public string guid;
            public string assemblyQualifiedTypeName;
        }

        private const string ADDRESSABLES_PATH_DICTIONARY_FILE_NAME = "AddressablesPathDictionary.asset";
        private const string LRAPI_RETURNS_FILE_NAME = "lrapi_returns.json";

        [Obsolete("utilize the \"instance\" property instead.")]
        public static AddressablesPathDictionary GetInstance()
        {
            return instance;
        }

        //This OnEnable will load all of the json data immediatly. This means that we can check the date-time of the file, and if its newer than the one we have serialized, we can flush out the cache.
        private void OnEnable()
        {
            EditorApplication.quitting += EditorApplication_quitting;
            LoadJSONFileData(out DateTime fileDateTime);

            if (fileDateTime > dictionaryCache.GetCacheCreationDateTime())
            {
                dictionaryCache.SetCacheDateTime(fileDateTime);
                RoR2EKLog.Debug("DateTime of JSON file is greater than serialized one, flushing out cache.");
                dictionaryCache.FlushOutCache();
            }

            //Delete the old one from the old path if it exists
            string oldPath = IOPath.Combine(R2EKConstants.FolderPaths.r2EKProjectSettingsPath, ADDRESSABLES_PATH_DICTIONARY_FILE_NAME);
            if(File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }
        }

        private void OnDisable()
        {
            EditorApplication.quitting -= EditorApplication_quitting;
        }

        private void EditorApplication_quitting()
        {
            SaveDictionary();
        }

        public void SaveDictionary() => Save(true);

        private void LoadJSONFileData(out DateTime fileDateTime)
        {
            var stopwatch = Stopwatch.StartNew();

            string jsonData = GetLRAPIReturnsJsonData(GetLRAPIReturnsPath(), out fileDateTime);

            var rootJSONNode = JSON.Parse(jsonData);

            Dictionary<string, Entry> guidToEntries = new();
            Dictionary<string, Entry> pathToEntries = new();
            List<Entry> entries = new();

            int randomAssNumber = 0;
            bool checkedForProperVersionOfLRAPI = false;
            foreach (var keyGUID in rootJSONNode.Keys)
            {
                if (wwiseRegex.Match(keyGUID).Success)
                {
                    continue;
                }

                Entry entry = null;
                try
                {
                    JSONNode entryNode = rootJSONNode[keyGUID];

                    JSONNode pathNode = entryNode["path"];
                    JSONNode assemblyQualifiedTypeNameNode = entryNode["assemblyQualifiedTypeName"];

                    entry = new Entry
                    {
                        path = pathNode.Value,
                        guid = keyGUID,
                        assemblyQualifiedTypeName = assemblyQualifiedTypeNameNode.Value
                    };
                }
                catch(Exception ex)
                {
                    string guidToAttemptToParse = keyGUID;
                    if(subAssetExtractor.Match(keyGUID).Success)
                    {
                        guidToAttemptToParse = subAssetExtractor.Replace(keyGUID, "");
                    }
                    if(!GUID.TryParse(guidToAttemptToParse, out _))
                    {
                        EditorUtility.DisplayDialog($"Exception during LRAPI_Returns parsing.", "It appears that your LRAPI_Returns file located in your game's streaming assets is outdated, this is because the keys are asset paths instead of GUIDS.\n\nThis is because either you're using a version of the game older than 1.4.1, or because LRAPI_Returns for this version of the game is missing. You can download a newer version of LRAPI_Returns on the modding discord if necessary.", "Ok");
                    }
                    else
                    {
                        EditorUtility.DisplayDialog($"Unknown Exception during LRAPI_Returns parsing.", "An unknown exception happened while LRAPI_Returns was being parsed, please submit a bug report", "Ok");
                    }
                    RoR2EKLog.Error(ex);
                    return;
                }

                if (!guidToEntries.TryAdd(entry.guid, entry))
                {
                    RoR2EKLog.Error($"A GUID to Entry was attempted to be added, but the key is already in the dictionary! (key={entry.guid},path={entry.path})");
                }
                if (!pathToEntries.TryAdd(entry.path, entry))
                {
                    //Path conflict resolution part 1, include typeName
                    Type assetType = Type.GetType(entry.assemblyQualifiedTypeName);
                    entry.path = string.Format("{0}:{1}", entry.path, assetType.Name);
                    if (!pathToEntries.TryAdd(entry.path, entry))
                    {
                        //Path conflict resolution part 2, just put a number.
                        entry.path = string.Format("{0}*{1}", entry.path, randomAssNumber);
                        randomAssNumber++;
                        if (!pathToEntries.TryAdd(entry.path, entry))
                        {
                            RoR2EKLog.Error($"A Path to Entry was attempted to be added, but the key is already in the dictionary! (key={entry.path},guid={entry.guid})");
                        }
                        else
                        {
                            RoR2EKLog.Warning($"Path to Entry was added after including Type name and Number, (key={entry.path}, guid={entry.guid}");
                        }
                    }
                    else
                    {
                        RoR2EKLog.Warning($"Path to Entry was added after including Type name, (key={entry.path}, guid={entry.guid}");
                    }

                }
                entries.Add(entry);
            }

            //Initialize the inner dictionaries with the new values.
            pathToEntryDictionary = pathToEntries;
            guidToEntryDictionary = guidToEntries;
            allEntries = entries.ToArray();

            paths = allEntries.Select(entry => entry.path).ToArray();
            guids = allEntries.Select(entry => entry.guid).ToArray();

            stopwatch.Stop();
            RoR2EKLog.Debug($"AddressablesPathDictionary took " + stopwatch.ElapsedMilliseconds + "ms");
        }

        private static string GetLRAPIReturnsPath()
        {
            R2EKPreferences preferencesInstance = R2EKPreferences.instance;
            var streamingAssetsFolder = preferencesInstance.streamingAssetsPath;
            var filePath = IOPath.Combine(streamingAssetsFolder, LRAPI_RETURNS_FILE_NAME);
            return filePath;
        }

        private static string GetLRAPIReturnsJsonData(string jsonFilePath, out DateTime fileDateTime)
        {
            fileDateTime = default;

            //Make it just use the R2EK one, revert to the original one later.
            TextAsset lrapiReturns1dot4dot1TextAsset = R2EKConstants.AssetGUIDs.lrapiReturnsFor1dot4dot1;
            if (lrapiReturns1dot4dot1TextAsset)
            {
                jsonFilePath = IOPath.GetFullPath(AssetDatabase.GetAssetPath(lrapiReturns1dot4dot1TextAsset));
                fileDateTime = File.GetLastWriteTimeUtc(jsonFilePath);
                return lrapiReturns1dot4dot1TextAsset.text;
            }

            /*if (File.Exists(jsonFilePath))
            {
                fileDateTime = File.GetLastWriteTimeUtc(jsonFilePath);
                return System.IO.File.ReadAllText(jsonFilePath);
            }
            else
            {
                TextAsset lrapiReturns1dot4dot1TextAsset = R2EKConstants.AssetGUIDs.lrapiReturnsFor1dot4dot1;
                if(lrapiReturns1dot4dot1TextAsset)
                {
                    jsonFilePath = IOPath.GetFullPath(AssetDatabase.GetAssetPath(lrapiReturns1dot4dot1TextAsset));
                    fileDateTime = File.GetLastWriteTimeUtc(jsonFilePath);
                    return lrapiReturns1dot4dot1TextAsset.text;
                }
            }*/

            RoR2EKLog.Fatal("LRAPI_RETURNS NOT FOUND!\n The json file lrapi_returns was not found, This version of RoR2EditorKit requires the game to be at the very least post memory management update.");
            return "";
        }

        private Dictionary<string, Entry> pathToEntryDictionary = new Dictionary<string, Entry>();
        private Dictionary<string, Entry> guidToEntryDictionary = new Dictionary<string, Entry>();
        private Entry[] allEntries = Array.Empty<Entry>();
        private string[] paths = Array.Empty<string>();
        private string[] guids = Array.Empty<string>();

        private Regex wwiseRegex = new Regex("Wwise");
        private Regex subAssetExtractor = new Regex(@"\[.*?\]");

        //Handles caching of entries.
        #region Cache
        [SerializeField]
        internal AddressablesPathDictionaryCache dictionaryCache = new AddressablesPathDictionaryCache();

        /// <summary>
        /// This method should not be called manually.
        /// </summary>
        public void OnBeforeSerialize()
        {
            dictionaryCache.OnBeforeSerialize();
        }

        /// <summary>
        /// This method should not be called manually.
        /// </summary>
        public void OnAfterDeserialize()
        {
            dictionaryCache.OnAfterSerialize();
        }

        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Invalidate AddressablesPathDictionary Cache")]
        private static void InvalidateCache()
        {
            var instance = AddressablesPathDictionary.instance;
            RoR2EKLog.Debug($"Flushing out {instance.dictionaryCache.GetCacheCount()} entries from the Cache.");
            instance.dictionaryCache.FlushOutCache();
            instance.SaveDictionary();
        }
        #endregion

        /// <summary>
        /// Returns true if the AddressablesPathDictionary holds no data whatsoever.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return (pathToEntryDictionary == null || pathToEntryDictionary.Count == 0) ||
                (guidToEntryDictionary == null || guidToEntryDictionary.Count == 0) ||
                (allEntries == null || allEntries.Length == 0);
        }

        /// <summary>
        /// Returns a ReadOnlyCollection of all the paths stored in the dictionary
        /// </summary>
        public ReadOnlyCollection<string> GetAllPaths()
        {

            return new ReadOnlyCollection<string>(paths);
        }

        /// <summary>
        /// Returns a ReadOnlyCollection of all the guids stored in the dictionary
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<string> GetAllGUIDS()
        {
            return new ReadOnlyCollection<string>(guids);
        }

        /// <summary>
        /// Tries to obtain a guid from it's addressable path.
        /// </summary>
        /// <param name="path">The addressable path, from which we want it's guid</param>
        /// <param name="guid">The resulting guid</param>
        /// <returns>True if the value was succesfuly obtained, otherwise false.</returns>
        public bool TryGetGUIDFromPath(string path, out string? guid)
        {
            guid = null;
            if (pathToEntryDictionary.TryGetValue(path, out Entry value))
            {
                guid = value.guid;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Obtains a guid from <paramref name="path"/> directly with no safety.
        /// </summary>
        /// <param name="path">The addressable path from which we want it's guid</param>
        /// <returns>The guid itself.</returns>
        public string GetGUIDFromPath(string path)
        {
            return pathToEntryDictionary[path].guid;
        }

        /// <summary>
        /// Tries to obtain a path from it's addressable guid.
        /// </summary>
        /// <param name="path">The resulting path</param>
        /// <param name="guid">The addressable guid, from which we want it's path</param>
        /// <returns>True if the value was succesfuly obtained, otherwise false.</returns>
        public bool TryGetPathFromGUID(string guid, out string? path)
        {
            path = null;
            if(guidToEntryDictionary.TryGetValue(guid, out Entry value))
            {
                path = value.path;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Obtains a path from <paramref name="guid"/> directly with no safety.
        /// </summary>
        /// <param name="guid">The addressable guid from which we want it's path</param>
        /// <returns>The path itself.</returns>
        public string GetPathFromGUID(string guid)
        {
            return guidToEntryDictionary[guid].path;
        }

        public bool TryGetTypeFromPath(string path, out Type type)
        {
            type = null;
            if(pathToEntryDictionary.TryGetValue(path, out Entry value))
            {
                type = Type.GetType(value.assemblyQualifiedTypeName);
            }
            return type != null;
        }

        public Type GetTypeFromPath(string path)
        {
            return Type.GetType(pathToEntryDictionary[path].assemblyQualifiedTypeName);
        }

        public bool TryGetTypeFromGUID(string guid, out Type type)
        {
            type = null;
            if (guidToEntryDictionary.TryGetValue(guid, out Entry value))
            {
                type = Type.GetType(value.assemblyQualifiedTypeName);
            }
            return type != null;
        }

        public Type GetTypeFromGUID(string guid)
        {
            return Type.GetType(guidToEntryDictionary[guid].assemblyQualifiedTypeName);
        }

        public IList GetSerializedCacheAsIList()
        {
            return dictionaryCache.serializedCacheList;
        }
        #region Obsolete
        [Obsolete("Create a new instance of \"EntryLookup\" and call \"PerformLookup\" instead")]
        public ReadOnlyArray<string> GetAllPathsOfType(Type t)
        {
            return GetAllPathsOfTypes(new Type[] { t });
        }


        [Obsolete("Create a new instance of \"EntryLookup\" and call \"PerformLookup\" instead")]
        public ReadOnlyArray<string> GetAllPathsOfTypes(Type[] types)
        {
            var lookup = new EntryLookup()
                .WithLookupType(EntryType.Path)
                .WithTypeRestriction(types);
            return new ReadOnlyArray<string>(lookup.PerformLookup().ToArray());
        }


        [Obsolete("Create a new instance of \"EntryLookup\" and call \"PerformLookup\" instead")]
        public ReadOnlyArray<string> GetAllGUIDSOfType(Type t)
        {
            return GetAllGUIDSOfTypes(new Type[] { t });
        }


        [Obsolete("Create a new instance of \"EntryLookup\" and call \"PerformLookup\" instead")]
        public ReadOnlyArray<string> GetAllGUIDSOfTypes(Type[] types)
        {
            var lookup = new EntryLookup()
                .WithLookupType(EntryType.Guid)
                .WithTypeRestriction(types);
            return new ReadOnlyArray<string>(lookup.PerformLookup().ToArray());
        }
        #endregion

        private AddressablesPathDictionary() { }
    }
}