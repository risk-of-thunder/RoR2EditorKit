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
using IOPath = System.IO.Path;
#if R2EK_ADDRESSABLES
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

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
            /// Whenether base game addressables should be included in the lookup results
            /// </summary>
            public bool includeBaseGameAddressables = true;

            /// <summary>
            /// Whenether project addressables should be included in the lookup results when the Addressables package is properly installed
            /// </summary>
            public bool includeProjectAddressables = true;

            /// <summary>
            /// The results of the lookup, this is populated after <see cref="PerformLookup"/> or <see cref="PerformLookupAsync"/>
            /// </summary>
            public ReadOnlyCollection<string> results;
            private List<string> _results;

            /// <summary>
            /// An action to invoke when there's progress to report.
            /// </summary>
            public IProgress<float> progressReport;

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
                if (typeRestriction.Length == 0 && (componentRequirement != null && componentRequirement.IsSubclassOf(typeof(Component))))
                {
                    HG.ArrayUtils.ArrayAppend(ref typeRestriction, typeof(GameObject));
                }
                if (includeBaseGameAddressables)
                {
                    for (int i = 0; i < typeRestriction.Length; i++) //Get each result for each type, request entries returns the cache if it exists, and if not, it builds it
                    {
                        float minProgress = ((float)i) / typeRestriction.Length;
                        float maxProgress = ((float)i + 1) / typeRestriction.Length;
                        yield return null;
                        Type type = typeRestriction[i];
                        string[] cache = instance.dictionaryCache.RequestEntries(type, componentRequirement, searchComponentInChildren, entryLookupType);
                        var modulo = Mathf.Floor((Mathf.Log10(cache.Length) + 1) * 2);
                        for (int j = 0; j < cache.Length; j++)
                        {
                            string cacheEntry = cache[j];
                            if (i % modulo == 0) yield return null;
                            progressReport?.Report(R2EKMath.Remap(j, 0, cache.Length, minProgress, maxProgress)); //If we have a filter, filter the results
                            if (!string.IsNullOrWhiteSpace(filter))
                            {
                                if (cacheEntry.Contains(filter) && !_results.Contains(cacheEntry)) _results.Add(cacheEntry);
                            }
                            else
                            {
                                if (!_results.Contains(cacheEntry)) _results.Add(cacheEntry);
                            }
                        }
                    }
                }
#if R2EK_ADDRESSABLES
                if (includeProjectAddressables) instance.CollectProjectLookupEntries(typeRestriction, componentRequirement, searchComponentInChildren, entryLookupType, filter, _results);
#endif
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

            public EntryLookup WithTypeRestriction<T>()
            {
                typeRestriction = new Type[] { typeof(T) };
                return this;
            }

            public EntryLookup WithComponentRequirement(Type componentRequirement, bool searchComponentInChildren)
            {
                this.componentRequirement = componentRequirement;
                this.searchComponentInChildren = searchComponentInChildren;
                return this;
            }

            public EntryLookup WithComponentRequirement<T>(bool searchComponentInChildren)
            {
                this.componentRequirement = typeof(T);
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

            public EntryLookup WithProgressReport(IProgress<float> progressReport)
            {
                this.progressReport = progressReport;
                return this;
            }

            public EntryLookup WithBaseGameAddressables(bool include)
            {
                includeBaseGameAddressables = include;
                return this;
            }

            public EntryLookup WithProjectAddressables(bool include)
            {
                includeProjectAddressables = include;
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

            public EntryLookup()
            {
                _results = ListPool<string>.RentCollection();
            }
        }

        private class Entry
        {
            public string path;
            public string guid;
            public string assemblyQualifiedTypeName;
            public string typeName;
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

#if R2EK_ADDRESSABLES
            _projectEntriesDirty = true;
            AddressableAssetSettings.OnModificationGlobal += OnAddressableSettingsModified;
            EditorApplication.projectChanged += MarkProjectEntriesDirty;
#endif
        }

        private void OnDisable()
        {
            EditorApplication.quitting -= EditorApplication_quitting;
#if R2EK_ADDRESSABLES
            AddressableAssetSettings.OnModificationGlobal -= OnAddressableSettingsModified;
            EditorApplication.projectChanged -= MarkProjectEntriesDirty;
#endif
        }

        private void EditorApplication_quitting()
        {
            SaveDictionary();
        }

        public void SaveDictionary() => Save(true);

        private void LoadJSONFileData(out DateTime fileDateTime)
        {
            using var stringBuilderRental = HG.StringBuilderPool.RentStringBuilder(out var sb);
            
            var stopwatch = Stopwatch.StartNew();

            using(new RoR2EKLog.StringBuilderRedirectorDisposable(sb))
            {

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
                        JSONNode typeName = entryNode["typeName"];

                        entry = new Entry
                        {
                            path = pathNode.Value,
                            guid = keyGUID,
                            assemblyQualifiedTypeName = assemblyQualifiedTypeNameNode.Value,
                            typeName = typeName
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

                    Type assetType = Type.GetType(entry.assemblyQualifiedTypeName);
                    if(assetType == null)
                    {
                        RoR2EKLog.Debug($"Skipping entry with guid \"{entry.guid}\" and path \"{entry.path}\" as it's Type resolved to null");
                        continue;
                    }

                    if (!guidToEntries.TryAdd(entry.guid, entry))
                    {
                        RoR2EKLog.Error($"A GUID to Entry was attempted to be added, but the key is already in the dictionary! (key={entry.guid},path={entry.path})");
                    }
                    if (!pathToEntries.TryAdd(entry.path, entry))
                    {
                        //Path conflict resolution part 1, include typeName

                        entry.path = string.Format("{0}:{1}", entry.path, assetType?.Name);
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
            }
            RoR2EKLog.Debug($"Printing AddressablesPathDictionary loading log.");
            RoR2EKLog.Debug(sb.ToString());
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

            if (File.Exists(jsonFilePath))
            {
                fileDateTime = File.GetLastWriteTimeUtc(jsonFilePath);
                return System.IO.File.ReadAllText(jsonFilePath);
            }

            RoR2EKLog.Message("LRAPI_RETURNS NOT FOUND!\n The json file lrapi_returns was not found, This version of RoR2EditorKit requires the game to be at the very least post memory management update.");
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
#if R2EK_ADDRESSABLES
            instance.MarkProjectEntriesDirty();
#endif
            instance.SaveDictionary();
        }
        #endregion

#if R2EK_ADDRESSABLES
        #region Project Addressables
        private class ProjectEntry
        {
            public string path;
            public string guid;
            public string assetPath;
            public Type assetType;
            public bool isSubAsset;
        }
        private readonly Dictionary<string, ProjectEntry> _projectPathToEntry = new Dictionary<string, ProjectEntry>();
        private readonly Dictionary<string, ProjectEntry> _projectGuidToEntry = new Dictionary<string, ProjectEntry>();
        private readonly List<ProjectEntry> _projectEntries = new List<ProjectEntry>();
        private string[] _combinedPaths = Array.Empty<string>();
        private string[] _combinedGuids = Array.Empty<string>();
        private bool _projectEntriesDirty = true;
        private void OnAddressableSettingsModified(AddressableAssetSettings settings, AddressableAssetSettings.ModificationEvent modificationEvent, object eventData) => MarkProjectEntriesDirty();
        private void MarkProjectEntriesDirty() => _projectEntriesDirty = true;
        private static AddressableAssetSettings GetProjectAddressablesSettings()
        {
            if (!AddressableAssetSettingsDefaultObject.SettingsExists) return null;
            return AddressableAssetSettingsDefaultObject.Settings;
        }
        private static string NormalizeProjectPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            path = path.Replace('\\', '/');
            while (path.Contains("//")) path = path.Replace("//", "/");
            return path.Trim('/');
        }
        private void EnsureProjectEntriesLoaded()
        {
            if (!_projectEntriesDirty) return;
            RefreshProjectEntries();
        }
        private void RefreshProjectEntries()
        {
            _projectPathToEntry.Clear();
            _projectGuidToEntry.Clear();
            _projectEntries.Clear();
            _combinedPaths = Array.Empty<string>();
            _combinedGuids = Array.Empty<string>();
            AddressableAssetSettings settings = GetProjectAddressablesSettings();
            if (settings == null)
            {
                if (!EditorApplication.isUpdating && !EditorApplication.isCompiling) _projectEntriesDirty = false;
                return;
            }
            _projectEntriesDirty = false;
            List<AddressableAssetEntry> allAssets = new List<AddressableAssetEntry>();
            try
            {
                settings.GetAllAssets(allAssets, true);
            }
            catch (Exception ex)
            {
                RoR2EKLog.Warning($"Failed to gather project Addressables entries: {ex.Message}");
                return;
            }
            int conflictNumber = 0;
            for (int i = 0; i < allAssets.Count; i++)
            {
                AddressableAssetEntry aaEntry = allAssets[i];
                if (aaEntry == null) continue;
                if (!string.IsNullOrEmpty(aaEntry.guid))
                {
                    string entryAssetPath = aaEntry.AssetPath;
                    if (string.IsNullOrEmpty(entryAssetPath) || AssetDatabase.IsValidFolder(entryAssetPath)) continue;
                    Type entryType = AssetDatabase.GetMainAssetTypeAtPath(entryAssetPath);
                    if (entryType == null) continue;
                    string rawPath = string.IsNullOrEmpty(aaEntry.address) ? entryAssetPath : aaEntry.address;
                    AddProjectEntry(rawPath, aaEntry.guid, entryAssetPath, entryType, false, ref conflictNumber);
                    if (aaEntry.IsSubAsset && entryType == typeof(Texture2D))
                    {
                        var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(entryAssetPath);
                        for (int s = 0; s < subAssets.Length; s++)
                        {
                            var subAsset = subAssets[s];
                            if (subAsset != null)
                            {
                                AddProjectEntry($"{rawPath}[{subAsset.name}]", $"{aaEntry.guid}[{subAsset.name}]", entryAssetPath, subAsset.GetType(), true, ref conflictNumber);
                            }
                        }
                    }
                }
                else if (aaEntry.IsSubAsset && aaEntry.ParentEntry != null && !string.IsNullOrEmpty(aaEntry.ParentEntry.guid))
                {
                    string braaap = aaEntry.address;
                    if (string.IsNullOrEmpty(braaap)) continue;
                    int subStart = braaap.LastIndexOf('[');
                    int subEnd = braaap.LastIndexOf(']');
                    if (subStart < 0 || subEnd <= subStart) continue;
                    string subSuffix = braaap.Substring(subStart, subEnd - subStart + 1);
                    string entryGuid = aaEntry.ParentEntry.guid + subSuffix;
                    string entryAssetPath = aaEntry.ParentEntry.AssetPath;
                    var targetAsset = aaEntry.TargetAsset;
                    Type entryType = targetAsset != null ? targetAsset.GetType() : null;
                    AddProjectEntry(braaap, entryGuid, entryAssetPath, entryType, true, ref conflictNumber);
                }
            }
            if (_projectEntries.Count > 0)
            {
                _combinedPaths = new string[paths.Length + _projectEntries.Count];
                Array.Copy(paths, 0, _combinedPaths, 0, paths.Length);
                _combinedGuids = new string[guids.Length + _projectEntries.Count];
                Array.Copy(guids, 0, _combinedGuids, 0, guids.Length);
                for (int i = 0; i < _projectEntries.Count; i++)
                {
                    _combinedPaths[paths.Length + i] = _projectEntries[i].path;
                    _combinedGuids[guids.Length + i] = _projectEntries[i].guid;
                }
            }
        }
        private void AddProjectEntry(string rawPath, string entryGuid, string entryAssetPath, Type entryType, bool isSubObject, ref int conflictNumber)
        {
            if (string.IsNullOrEmpty(entryGuid) || entryType == null) return;
            string entryPath = NormalizeProjectPath(rawPath);
            if (string.IsNullOrEmpty(entryPath)) return;
            if (guidToEntryDictionary.ContainsKey(entryGuid) || _projectGuidToEntry.ContainsKey(entryGuid)) return;
            string uniquePath = entryPath;
            if (pathToEntryDictionary.ContainsKey(uniquePath) || _projectPathToEntry.ContainsKey(uniquePath))
            {
                uniquePath = $"{entryPath}:{entryType.Name}";
                while (pathToEntryDictionary.ContainsKey(uniquePath) || _projectPathToEntry.ContainsKey(uniquePath))
                {
                    uniquePath = $"{entryPath}:{entryType.Name}*{conflictNumber}";
                    conflictNumber++;
                }
            }
            ProjectEntry projectEntry = new ProjectEntry
            {
                path = uniquePath,
                guid = entryGuid,
                assetPath = entryAssetPath,
                assetType = entryType,
                isSubAsset = isSubObject
            };
            _projectGuidToEntry.Add(projectEntry.guid, projectEntry);
            _projectPathToEntry.Add(projectEntry.path, projectEntry);
            _projectEntries.Add(projectEntry);
        }
        private bool MatchesProjectEntry(ProjectEntry entry, Type type, Type componentRequirement, bool searchInChildren)
        {
            Type requiredType = componentRequirement ?? type;
            if (requiredType == null) return true;
            if (requiredType == typeof(Component) || requiredType.IsSubclassOf(typeof(Component)))
            {
                if (entry.assetType != typeof(GameObject)) return false;
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(entry.assetPath);
                if (!gameObject) return false;
                return searchInChildren ? gameObject.GetComponentInChildren(requiredType) != null : gameObject.TryGetComponent(requiredType, out _);
            }
            return requiredType.IsAssignableFrom(entry.assetType);
        }
        internal void CollectProjectLookupEntries(Type[] typeRestriction, Type componentRequirement,  bool searchInChildren, EntryType entryLookupType, string filter, List<string> destination)
        {
            EnsureProjectEntriesLoaded();
            if (_projectEntries.Count == 0) return;
            bool hasFilter = !string.IsNullOrWhiteSpace(filter);
            HashSet<string> existing = new HashSet<string>(destination);
            int typeCount = typeRestriction != null ? typeRestriction.Length : 0;
            for (int i = 0; i < _projectEntries.Count; i++)
            {
                ProjectEntry entry = _projectEntries[i];
                string value = entryLookupType switch
                {
                    EntryType.Path => entry.path,
                    EntryType.Guid => entry.guid,
                    _ => null
                };
                if (string.IsNullOrEmpty(value) || existing.Contains(value)) continue;
                if (hasFilter && !value.Contains(filter)) continue;
                bool matched = false;
                if (typeCount == 0)
                {
                    if (componentRequirement != null)
                    {
                        matched = MatchesProjectEntry(entry, null, componentRequirement, searchInChildren);
                    }
                }
                else
                {
                    for (int t = 0; t < typeCount; t++)
                    {
                        if (MatchesProjectEntry(entry, typeRestriction[t], componentRequirement, searchInChildren))
                        {
                            matched = true;
                            break;
                        }
                    }
                }
                if (matched)
                {
                    existing.Add(value);
                    destination.Add(value);
                }
            }
        }
        #endregion
#endif
        internal ReadOnlyCollection<string> GetBaseGameGUIDs() => new ReadOnlyCollection<string>(guids);

        private readonly Dictionary<Type, Texture2D> _typeIconCache = new Dictionary<Type, Texture2D>();

        private Texture2D GetTypeIcon(Type assetType)
        {
            if (assetType == null) return null;
            if (_typeIconCache.TryGetValue(assetType, out Texture2D cached)) return cached;
            Texture2D icon = AssetPreview.GetMiniTypeThumbnail(assetType);
            if (icon == null)
            {
                GUIContent content = EditorGUIUtility.ObjectContent(null, assetType);
                icon = content?.image as Texture2D;
            }
            _typeIconCache[assetType] = icon;
            return icon;
        }

        public Texture2D GetPreviewIconFromGUID(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
#if R2EK_ADDRESSABLES
            EnsureProjectEntriesLoaded();
            if (_projectGuidToEntry.TryGetValue(guid, out ProjectEntry projectEntry))
            {
                if (!string.IsNullOrEmpty(projectEntry.assetPath))
                {
                    if (projectEntry.assetType != null && typeof(Material).IsAssignableFrom(projectEntry.assetType))
                    {
                        Material material = AssetDatabase.LoadAssetAtPath<Material>(projectEntry.assetPath);
                        if (material != null)
                        {
                            Texture2D materialPreview = AssetPreview.GetAssetPreview(material);
                            if (materialPreview != null) return materialPreview;
                        }
                    }
                    Texture2D cachedIcon = AssetDatabase.GetCachedIcon(projectEntry.assetPath) as Texture2D;
                    if (cachedIcon != null) return cachedIcon;
                }
                return GetTypeIcon(projectEntry.assetType);
            }
#endif
            if (TryGetTypeFromGUID(guid, out Type assetType)) return GetTypeIcon(assetType);
            return null;
        }
        /// <summary>
        /// Returns true if the AddressablesPathDictionary holds no data whatsoever.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            bool baseEmpty = (pathToEntryDictionary == null || pathToEntryDictionary.Count == 0) ||
                (guidToEntryDictionary == null || guidToEntryDictionary.Count == 0) ||
                (allEntries == null || allEntries.Length == 0);
#if R2EK_ADDRESSABLES
            if (baseEmpty)
            {
                EnsureProjectEntriesLoaded();
                return _projectEntries.Count == 0;
            }
#endif
            return baseEmpty;
        }

        /// <summary>
        /// Returns true if the specified GUID is in the <see cref="AddressablesPathDictionary"/>
        /// </summary>
        public bool IsGUIDValid(string assetGUID)
        {
            if (string.IsNullOrEmpty(assetGUID)) return false;
            if (guidToEntryDictionary.ContainsKey(assetGUID)) return true;
#if R2EK_ADDRESSABLES
            EnsureProjectEntriesLoaded();
            return _projectGuidToEntry.ContainsKey(assetGUID);
#else
            return false;
#endif
        }

        /// <summary>
        /// Returns a ReadOnlyCollection of all the paths stored in the dictionary
        /// </summary>
        public ReadOnlyCollection<string> GetAllPaths()
        {
#if R2EK_ADDRESSABLES
            EnsureProjectEntriesLoaded();
            if (_projectEntries.Count > 0) return new ReadOnlyCollection<string>(_combinedPaths);
#endif
            return new ReadOnlyCollection<string>(paths);
        }

        /// <summary>
        /// Returns a ReadOnlyCollection of all the guids stored in the dictionary
        /// </summary>
        /// <returns></returns>
        public ReadOnlyCollection<string> GetAllGUIDS()
        {
#if R2EK_ADDRESSABLES
            EnsureProjectEntriesLoaded();
            if (_projectEntries.Count > 0) return new ReadOnlyCollection<string>(_combinedGuids);
#endif
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
            if (string.IsNullOrEmpty(path))
                return false;

            if (pathToEntryDictionary.TryGetValue(path, out Entry value))
            {
                guid = value.guid;
                return true;
            }
#if R2EK_ADDRESSABLES
            EnsureProjectEntriesLoaded();
            if (_projectPathToEntry.TryGetValue(path, out ProjectEntry projectValue))
            {
                guid = projectValue.guid;
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Obtains a guid from <paramref name="path"/>, returns an empty string if the path is not in the AddressablesPathDictionary
        /// </summary>
        /// <param name="path">The addressable path from which we want it's guid</param>
        public string GetGUIDFromPath(string path)
        {
            if(TryGetGUIDFromPath(path, out string guid)) return guid;
            RoR2EKLog.Warning($"Path {path} is not in the AddressablesPathDictionary");
            return string.Empty;
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
            if (string.IsNullOrEmpty(guid)) return false;
            if(guidToEntryDictionary.TryGetValue(guid, out Entry value))
            {
                path = value.path;
                return true;
            }
#if R2EK_ADDRESSABLES
            EnsureProjectEntriesLoaded();
            if (_projectGuidToEntry.TryGetValue(guid, out ProjectEntry projectValue))
            {
                path = projectValue.path;
                return true;
            }
#endif
            return false;
        }

        /// <summary>
        /// Obtains a path from <paramref name="guid"/>, returns an empty string if the guid is not in the AddressablesPathDictionary
        /// </summary>
        /// <param name="guid">The addressable guid from which we want it's path</param>
        public string GetPathFromGUID(string guid)
        {
            if(TryGetPathFromGUID(guid, out string? path)) return path;
            RoR2EKLog.Warning($"GUID {guid} is not in the AddressablesPathDictionary");
            return string.Empty;
        }

        /// <summary>
        /// Attempts to get the Type of asset that <paramref name="path"/> points to
        /// </summary>
        /// <param name="path">The Path of the asset</param>
        /// <param name="type">The Type of the Asset which <paramref name="guid"/> represents</param>
        /// <returns>True if a Type was found, otherwise false.</returns>
        public bool TryGetTypeFromPath(string path, out Type type)
        {
            type = null;
            if (string.IsNullOrEmpty(path)) return false;
            if(pathToEntryDictionary.TryGetValue(path, out Entry value)) type = Type.GetType(value.assemblyQualifiedTypeName);
#if R2EK_ADDRESSABLES
            if (type == null)
            {
                EnsureProjectEntriesLoaded();
                if (_projectPathToEntry.TryGetValue(path, out ProjectEntry projectValue)) type = projectValue.assetType;
            }
#endif
            return type != null;
        }

        /// <summary>
        /// Returns the Type of asset that <paramref name="path"/> points to, returns null if the path is not in the AddressablesPathDictionary
        /// </summary>
        /// <param name="path">The path of the asset to get it's type</param>
        public Type GetTypeFromPath(string path)
        {
            if(TryGetTypeFromPath(path, out Type type)) return type;
            RoR2EKLog.Warning($"Path {path} is not in the AddressablesPathDictionary");
            return null;
        }

        /// <summary>
        /// Attempts to get the Type of asset that <paramref name="guid"/> points to
        /// </summary>
        /// <param name="guid">The GUID of the asset</param>
        /// <param name="type">The Type of the Asset which <paramref name="guid"/> represents</param>
        /// <returns>True if a Type was found, otherwise false.</returns>
        public bool TryGetTypeFromGUID(string guid, out Type type)
        {
            type = null;
            if (string.IsNullOrEmpty(guid)) return false;
            if (guidToEntryDictionary.TryGetValue(guid, out Entry value)) type = Type.GetType(value.assemblyQualifiedTypeName);
#if R2EK_ADDRESSABLES
            if (type == null)
            {
                EnsureProjectEntriesLoaded();
                if (_projectGuidToEntry.TryGetValue(guid, out ProjectEntry projectValue)) type = projectValue.assetType;
            }
#endif
            return type != null;
        }

        /// <summary>
        /// Returns the Type of asset that <paramref name="guid"/> points to, returns null if the guid is not in the AddressablesPathDictionary
        /// </summary>
        /// <param name="guid">The guid of the asset to get it's type</param>
        public Type GetTypeFromGUID(string guid)
        {
            if (TryGetTypeFromGUID(guid, out Type type)) return type;
            RoR2EKLog.Warning($"GUID {guid} is not in the AddressablesPathDictionary");
            return null;
        }

        /// <summary>
        /// Access to the Serialized Dictionary as an IList, this should not be modified as it's exposed exclusively for VisualElements
        /// </summary>
        public IList GetSerializedCacheAsIList()
        {
            return dictionaryCache.serializedCacheList;
        }

        /// <summary>
        /// Opens the ProjectSettings window and selects the Addressables Path Dictionary
        /// </summary>
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Project/PerUser/Addressable Dictionary Cache View")]
        public static void OpenCacheView() => SettingsService.OpenProjectSettings("Project/RoR2EditorKit/PerUser/Addressables Path Dictionary");
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