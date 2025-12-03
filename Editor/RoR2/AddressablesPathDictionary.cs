using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using HG;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RoR2.Editor
{
    /// <summary>
    /// The AddressablesPathDictionary is a struct that contains the metadata stored within the game's "lrapi_returns.json", which is a Dictionary of Addressable Path to GUID.
    /// </summary>
    public sealed class AddressablesPathDictionary
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
                while(sync.MoveNext())
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

                //Get each result for each type, request entries returns the cache if it exists, and if not, it builds it
                foreach(Type type in typeRestriction)
                {
                    yield return null;
                    string[] cache = AddressablesPathDictionary.GetInstance().RequestEntries(type, entryLookupType);


                    var modulo = Mathf.Floor((Mathf.Log10(cache.Length) + 1) * 2);
                    for (int i = 0; i < cache.Length; i++)
                    {
                        string cacheEntry = cache[i];

                        if(i % modulo == 0)
                        {
                            yield return null;
                        }

                        //If we have a filter, filter the results
                        if (!filter.IsNullOrEmptyOrWhiteSpace())
                        {
                            if(cacheEntry.Contains(filter) && !_results.Contains(cacheEntry))
                            {
                                _results.Add(cacheEntry);
                            }
                        }
                        else
                        {
                            if(!_results.Contains(cacheEntry))
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

            var locator = Addressables.ResourceLocators.FirstOrDefault(l => l.LocatorId == "AddressablesMainContentCatalog") as ResourceLocationMap;
            if (locator == null)
            {
                if (EditorUtility.DisplayDialog("RoR2EditorKit Addressables", 
                    "Couldn't find game addressable main content catalog resource location map. " +
                    "This version of RoR2EditorKit requires the game to be at least the version that introduced addressable usage. " +
                    "The editor will now close.", 
                    "Ok"))
                {
                    EditorApplication.Exit(0);
                    return _instance;
                }
            }

            var guidRegex = new Regex("^[0-9a-f]{32}$", RegexOptions.Compiled);

            Dictionary<string, string> pathToGUID = new Dictionary<string, string>();
            Dictionary<string, string> guidToPath = new Dictionary<string, string>();

            foreach (var (key, locations) in locator.Locations)
            {
                //There are at least 3 different key formats in the locator, filtering for guids
                if (key is not string guid || guid.Length != 32 || !guidRegex.IsMatch(guid))
                {
                    continue;
                }

                foreach (var location in locations)
                {
                    pathToGUID[location.PrimaryKey] = guid;
                    guidToPath[guid] = location.PrimaryKey;
                }
            }

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

        #region Cache
        private struct CacheHit
        {
            public string[] pathCache;
            public string[] guidsCache;
        }

        private Dictionary<Type, CacheHit> _typeResultCache;

        private string[] RequestEntries(Type type, EntryType entryType)
        {
            //First, we check if we have the cache
            if(_typeResultCache.TryGetValue(type, out CacheHit cacheHit))
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
            cacheHit = BuildCacheForType(type);
            return entryType switch
            {
                EntryType.Path => cacheHit.pathCache,
                EntryType.Guid => cacheHit.guidsCache,
                _ => Array.Empty<string>()
            };
        }

        private CacheHit BuildCacheForType(Type type)
        {
            using var _0 = ListPool<string>.RentCollection(out var resultPaths);
            using var _1 = ListPool<string>.RentCollection(out var resultGuids);

            //Work off GUIDS, like god intended
            foreach(var guid in guids)
            {
                var resourceLocations = Addressables.LoadResourceLocationsAsync(guid).WaitForCompletion();

                //There's a chance that there's no resources at this guid... idk???
                if (resourceLocations.Count == 0)
                {
                    UnityEngine.Debug.Log($"No resources located at {GetPathFromGUID(guid)}");
                    continue;
                }

                /* This is a bit fucky, but basically there's an issue where all sub-asset guids (the ones that have [] at the end) always has a pointer
                *  back to its main asset, it's odd because even the non [] ones have it as well... specifically for sprites?
                *  Anyways i need to figure some shit out
                */

                //If it contains the [, then stritctly match a sub-asset (at the very least the 2nd location).
                bool strictSubAssetMatch = guid.Contains("[");
                int resourceLocationIndex = -1;
                if(strictSubAssetMatch && resourceLocations.Count > 1)
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
                catch(Exception e)
                {
                    UnityEngine.Debug.LogException(e);
                    throw;
                }
                var resourceType = resourceLocation.ResourceType;

                //If we obtained the resource type, and the resource type is same or subclass of type, add it to the result
                if (resourceType != null && (resourceType == type || resourceType.IsSubclassOf(type)))
                {
                    resultGuids.Add(guid);
                    resultPaths.Add(GetPathFromGUID(guid));
                }
            }

            CacheHit cacheHit = new CacheHit();
            cacheHit.pathCache = resultPaths.ToArray();
            cacheHit.guidsCache = resultGuids.ToArray();

            //Save in the cache
            _typeResultCache.Add(type, cacheHit);

            return cacheHit;
        }
        #endregion

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
        private AddressablesPathDictionary(Dictionary<string, string> pathToGUID, Dictionary<string, string> guidToPath)
        {
            pathToGUIDDictionary = pathToGUID;
            paths = pathToGUID.Keys.ToArray();

            guidToPathDictionary = guidToPath;
            guids = guidToPath.Keys.ToArray();

            _typeResultCache = new Dictionary<Type, CacheHit>();
        }
    }
}