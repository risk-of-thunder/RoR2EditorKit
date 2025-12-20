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

namespace RoR2.Editor
{
    /// <summary>
    /// The AddressablesPathDictionary is a struct that contains the metadata stored within the game's "lrapi_returns.json", which is a Dictionary of Addressable Path to GUID.
    /// </summary>
    public sealed class AddressablesPathDictionary
    {
        [MenuItem("Epic/Test")]
        private static void Test()
        {
            ReadOnlyCollection<string> results = new EntryLookup()
                .WithComponentRequirement(typeof(CharacterBody), false)
                .WithLookupType(EntryType.Path)
                .WithTypeRestriction(typeof(GameObject))
                .PerformLookup();

            foreach(var entry in results)
            {
                Debug.Log(entry);
            }
        }
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
                    string[] cache = AddressablesPathDictionary.GetInstance().RequestEntries(type, componentRequirement, searchComponentInChildren, entryLookupType);


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

        private const string FILE_NAME = "lrapi_returns.json";

        /// <summary>
        /// Returns the instance currently stored.
        /// </summary>
        /// <returns>The instance of AddressablesPathDictionary</returns>
        public static AddressablesPathDictionary GetInstance()
        {
            if (_instance != null && !_instance.IsEmpty())
            {
                return _instance;
            }

            var stopwatch = Stopwatch.StartNew();
            string jsonData = GetLRAPIReturnsJsonData(GetLRAPIReturnsPath());

            var rootJSONNode = JSON.Parse(jsonData);

            var regex = new Regex("Wwise");

            Dictionary<string, Entry> guidToEntries = new();
            Dictionary<string, Entry> pathToEntries = new();
            List<Entry> entries = new();

            int randomAssNumber = 0;
            foreach(var keyGUID in rootJSONNode.Keys)
            {
                if(regex.Match(keyGUID).Success)
                {
                    continue;
                }

                JSONNode entryNode = rootJSONNode[keyGUID];

                JSONNode pathNode = entryNode["path"];
                JSONNode assemblyQualifiedTypeNameNode = entryNode["assemblyQualifiedTypeName"];

                Entry entry = new Entry
                {
                    path = pathNode.Value,
                    guid = keyGUID,
                    assemblyQualifiedTypeName = assemblyQualifiedTypeNameNode.Value
                };

                if(!guidToEntries.TryAdd(entry.guid, entry))
                {
                    Debug.LogError($"A GUID to Entry was attempted to be added, but the key is already in the dictionary! (key={entry.guid},path={entry.path})");
                }
                if(!pathToEntries.TryAdd(entry.path, entry))
                {
                    //Path conflict resolution part 1, include typeName
                    Type assetType = Type.GetType(entry.assemblyQualifiedTypeName);
                    entry.path = string.Format("{0}:{1}", entry.path, assetType.Name);
                    if(!pathToEntries.TryAdd(entry.path, entry))
                    {
                        //Path conflict resolution part 2, just put a number.
                        entry.path = string.Format("{0}*{1}", entry.path, randomAssNumber);
                        randomAssNumber++;
                        if(!pathToEntries.TryAdd(entry.path, entry))
                        {
                            Debug.LogError($"A Path to Entry was attempted to be added, but the key is already in the dictionary! (key={entry.path},guid={entry.guid})");
                        }
                        else
                        {
                            Debug.LogWarning($"Path to Entry was added after including Type name and Number, (key={entry.path}, guid={entry.guid}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Path to Entry was added after including Type name, (key={entry.path}, guid={entry.guid}");
                    }
                        
                }
                entries.Add(entry);
            }

            _instance = new AddressablesPathDictionary(pathToEntries, guidToEntries, entries.ToArray());
            stopwatch.Stop();
            UnityEngine.Debug.Log($"AddressablesPathDictionary took " + stopwatch.ElapsedMilliseconds + "ms");
            return _instance;
        }

        private static string GetLRAPIReturnsPath()
        {
            var gameExePath = R2EKPreferences.instance.GetGameExecutablePath();
            var directory = Directory.GetParent(gameExePath).FullName;
            var dataFolder = IOPath.Combine(directory, "Risk of Rain 2_Data");
            var streamingAssetsFolder = IOPath.Combine(dataFolder, "StreamingAssets");
            var filePath = IOPath.Combine(streamingAssetsFolder, FILE_NAME);
            return filePath;
        }

        private static string GetLRAPIReturnsJsonData(string jsonFilePath)
        {
            if (File.Exists(jsonFilePath))
            {
                return System.IO.File.ReadAllText(jsonFilePath);
            }
            else
            {
                TextAsset lrapiReturns1dot4dot1TextAsset = R2EKConstants.AssetGUIDs.lrapiReturnsFor1dot4dot1;
                if(lrapiReturns1dot4dot1TextAsset)
                {
                    return lrapiReturns1dot4dot1TextAsset.text;
                }
            }

            EditorUtility.DisplayDialog("LRAPI_RETURNS NOT FOUND", "The json file lrapi_returns was not found, This version of RoR2EditorKit requires the game to be at the very least post memory management update. The editor will now close.", "Ok");

            EditorApplication.Exit(0);
            return "";
        }

        private static AddressablesPathDictionary _instance;

        private Dictionary<string, Entry> pathToEntryDictionary;
        private Dictionary<string, Entry> guidToEntryDictionary;
        private Entry[] allEntries;
        private string[] paths;
        private string[] guids;

        #region Cache
        private struct CacheHit
        {
            public string[] pathCache;
            public string[] guidsCache;
        }

        private struct CacheKey : IEquatable<CacheKey>
        {
            public Type requiredType;
            public bool entriesIncludeComponentFoundInChildren;

            public bool Equals(CacheKey other)
            {
                return (requiredType == other.requiredType) &&
                    (entriesIncludeComponentFoundInChildren == other.entriesIncludeComponentFoundInChildren);
            }

            public override bool Equals(object obj)
            { 
                if(obj is CacheKey cacheKey)
                {
                    return this.Equals(cacheKey);
                }
                return false;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(requiredType.GetHashCode(), entriesIncludeComponentFoundInChildren.GetHashCode());
            }

            public static bool operator ==(CacheKey lhs, CacheKey rhs)
            {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(CacheKey lhs, CacheKey rhs)
            {
                return !(lhs == rhs);
            }
        }

        private Dictionary<CacheKey, CacheHit> _typeResultCache;

        private string[] RequestEntries(Type type, Type componentRequirement, bool searchInChildren, EntryType entryType)
        {
            //First, we check if we have the cache
            Type typeForCacheKey = componentRequirement ?? type;
            CacheKey cacheKey = new CacheKey { requiredType = typeForCacheKey, entriesIncludeComponentFoundInChildren = searchInChildren };

            if(_typeResultCache.TryGetValue(cacheKey, out CacheHit cacheHit))
            {
                //We've hit the cache, return results
                return entryType switch
                {
                    EntryType.Path => cacheHit.pathCache,
                    EntryType.Guid => cacheHit.guidsCache,
                    _ => Array.Empty<string>()
                };
            }

            //We missed the cache, so time to build it for this type and return as needed
            cacheHit = BuildCacheForType(typeForCacheKey, searchInChildren);
            return entryType switch
            {
                EntryType.Path => cacheHit.pathCache,
                EntryType.Guid => cacheHit.guidsCache,
                _ => Array.Empty<string>()
            };
        }

        private CacheHit BuildCacheForType(Type type, bool searchInChildren)
        {
            using var _0 = ListPool<string>.RentCollection(out var resultPaths);
            using var _1 = ListPool<string>.RentCollection(out var resultGuids);

            Type typeToCompareAssetTypeAgainst = type;
            if(typeToCompareAssetTypeAgainst.IsSubclassOf(typeof(Component)))
            {
                typeToCompareAssetTypeAgainst = typeof(GameObject);
            }
            //Work off GUIDS, like god intended
            foreach(var guid in guids)
            {
                Type assetType = GetAssetType(guid);
                //If we obtained the resource type, and the resource type is same or subclass of type, add it to the result
                if (assetType != null && (assetType == typeToCompareAssetTypeAgainst || assetType.IsSubclassOf(typeToCompareAssetTypeAgainst)))
                {
                    //We need to do an extra check if the type to compare against is GameObject, and the actual type we want is a component.
                    if(typeToCompareAssetTypeAgainst == typeof(GameObject) && type.IsSubclassOf(typeof(Component)))
                    {
                        GameObject gameObject = Addressables.LoadAssetAsync<GameObject>(guid).WaitForCompletion();
                        if(!gameObject)
                        {
                            continue;
                        }

                        //If it doesnt have the component we want, skip it.
                        bool hasComponent = searchInChildren ? gameObject.GetComponentInChildren(type) : gameObject.TryGetComponent(type, out _);
                        if (!hasComponent)
                            continue;
                    }

                    resultGuids.Add(guid);
                    resultPaths.Add(GetPathFromGUID(guid));
                }
            }

            CacheKey cacheKey = new CacheKey() { requiredType = type, entriesIncludeComponentFoundInChildren = searchInChildren };
            CacheHit cacheHit = new CacheHit();
            cacheHit.pathCache = resultPaths.ToArray();
            cacheHit.guidsCache = resultGuids.ToArray();

            //Save in the cache
            _typeResultCache.Add(cacheKey, cacheHit);

            return cacheHit;
        }

        private Type GetAssetType(string guid)
        {
            var resourceLocations = Addressables.LoadResourceLocationsAsync(guid).WaitForCompletion();

            //There's a chance that there's no resources at this guid... idk???
            if (resourceLocations.Count == 0)
            {
                //If this happens, return the type stored on the dictionary

                Entry entry = guidToEntryDictionary[guid];
                return Type.GetType(entry.assemblyQualifiedTypeName);
            }

            /* This is a bit fucky, but basically there's an issue where all sub-asset guids (the ones that have [] at the end) always has a pointer
            *  back to its main asset, it's odd because even the non [] ones have it as well... specifically for sprites?
            *  Anyways i need to figure some shit out
            */

            //If it contains the [, then stritctly match a sub-asset (at the very least the 2nd location).
            bool strictSubAssetMatch = guid.Contains("[");
            int resourceLocationIndex = -1;
            if (strictSubAssetMatch && resourceLocations.Count > 1)
            {
                resourceLocationIndex = 1;
            }
            else
            {
                resourceLocationIndex = 0;
            }

            IResourceLocation resourceLocation = null;
            try
            {
                resourceLocation = resourceLocations[resourceLocationIndex];
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                throw;
            }
            return resourceLocation.ResourceType;
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
        private AddressablesPathDictionary(Dictionary<string, Entry> pathToEntries, Dictionary<string, Entry> guidtoEntry, Entry[] allEntries)
        {
            pathToEntryDictionary = pathToEntries;
            guidToEntryDictionary = guidtoEntry;
            this.allEntries = allEntries;

            paths = allEntries.Select(entry => entry.path).ToArray();
            guids = allEntries.Select(entry => entry.guid).ToArray();

            _typeResultCache = new Dictionary<CacheKey, CacheHit>();
        }
    }
}