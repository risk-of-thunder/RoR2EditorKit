using HG;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace RoR2.Editor
{
    [Serializable]
    internal sealed class AddressablesPathDictionaryCache
    {
        [Serializable]
        private struct CacheKey : IEquatable<CacheKey>
        {
            public SerializableSystemType requiredType;
            public bool entriesIncludeComponentFoundInChildren;

            public bool Equals(CacheKey other)
            {
                return (requiredType == other.requiredType) &&
                    (entriesIncludeComponentFoundInChildren == other.entriesIncludeComponentFoundInChildren);
            }

            public override bool Equals(object obj)
            {
                if (obj is CacheKey cacheKey)
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

            public override string ToString()
            {
                StringBuilder sb = HG.StringBuilderPool.RentStringBuilder();
                sb.Append($"RequieredType:{((Type)requiredType).AssemblyQualifiedName} ");
                sb.Append($"Component is allowed to be in children?:{entriesIncludeComponentFoundInChildren}");
                string result = sb.ToString();
                HG.StringBuilderPool.ReturnStringBuilder(sb);
                return result;
            }
        }

        [Serializable]
        private struct CacheHit
        {
            public string[] pathCache;
            public string[] guidsCache;

            public bool isEmpty
            {
                get
                {
                    bool pathsEmpty = pathCache == null || pathCache.Length == 0;
                    bool guidsEmpty = guidsCache == null || guidsCache.Length == 0;

                    //Paths and Guids should be one to one, so an OR here makes sense.
                    return pathsEmpty || guidsEmpty;
                }
            }
        }

        [Serializable]
        private struct SerializedTypeResultCache
        {
            public CacheKey cacheKey;
            public CacheHit cacheHit;

            public SerializedTypeResultCache(CacheKey cacheKey, CacheHit cacheHit)
            {
                this.cacheKey = cacheKey;
                this.cacheHit = cacheHit;
            }
        }

        /// <summary>
        /// The Serialized Cache as an IList, this is exposed exclusively for VisualElement purposes and should not be touched directly.
        /// </summary>
        public IList serializedCacheList => _serializedTypeResultCache;
        [SerializeField] private long _cacheDateTimeTicks = DateTime.MinValue.Ticks;
        [SerializeField] private List<SerializedTypeResultCache> _serializedTypeResultCache = new List<SerializedTypeResultCache>();
        private Dictionary<CacheKey, CacheHit> _typeResultCache = new Dictionary<CacheKey, CacheHit>();

        internal string[] RequestEntries(Type type, Type componentRequirement, bool searchInChildren, AddressablesPathDictionary.EntryType entryType)
        {
            //First, we check if we have the cache
            Type typeForCacheKey = componentRequirement ?? type;
            CacheKey cacheKey = new CacheKey { requiredType = (SerializableSystemType)typeForCacheKey, entriesIncludeComponentFoundInChildren = searchInChildren };

            if (_typeResultCache.TryGetValue(cacheKey, out CacheHit cacheHit))
            {
                //We've hit the cache
                if (cacheHit.isEmpty)
                {
                    Debug.LogWarning($"Cache hit for key {cacheKey} is empty! rebuilding.");
                    _typeResultCache.Remove(cacheKey);
                    cacheHit = default;
                }
                else
                {
                    return entryType switch
                    {
                        AddressablesPathDictionary.EntryType.Path => cacheHit.pathCache,
                        AddressablesPathDictionary.EntryType.Guid => cacheHit.guidsCache,
                        _ => Array.Empty<string>()
                    };
                }
            }

            //We missed the cache, so time to build it for this type and return as needed
            cacheHit = BuildCacheForType(typeForCacheKey, searchInChildren);
            if (cacheHit.isEmpty)
            {
                Debug.LogError($"No addressables assets where found for cache key {cacheKey}");
                return Array.Empty<string>();
            }

            return entryType switch
            {
                AddressablesPathDictionary.EntryType.Path => cacheHit.pathCache,
                AddressablesPathDictionary.EntryType.Guid => cacheHit.guidsCache,
                _ => Array.Empty<string>()
            };
        }

        internal DateTime GetCacheCreationDateTime() => new DateTime(_cacheDateTimeTicks);
        internal void SetCacheDateTime(DateTime newDateTime) => _cacheDateTimeTicks = newDateTime.Ticks;

        internal void FlushOutCache()
        {
            for(int i = _serializedTypeResultCache.Count - 1; i >= 0; i--)
            {
                RemoveCache(i);
            }
        }

        internal void RemoveCache(Type type, bool searchInChildren)
        {
            CacheKey cacheKey = new CacheKey() { requiredType = (SerializableSystemType)type, entriesIncludeComponentFoundInChildren = searchInChildren };
            for(int i = 0; i < _serializedTypeResultCache.Count; i++)
            {
                if (_serializedTypeResultCache[i].cacheKey == cacheKey)
                {
                    RemoveCache(i);
                    break;
                }
            }
        }

        internal int GetCacheCount() => _serializedTypeResultCache.Count;

        private void RemoveCache(int serializedCacheIndex)
        {
            CacheKey cacheKey = _serializedTypeResultCache[serializedCacheIndex].cacheKey;
            _typeResultCache.Remove(cacheKey);
            _serializedTypeResultCache.RemoveAt(serializedCacheIndex);
        }

        private CacheHit BuildCacheForType(Type type, bool searchInChildren)
        {
            using var _0 = ListPool<string>.RentCollection(out var resultPaths);
            using var _1 = ListPool<string>.RentCollection(out var resultGuids);

            Type typeToCompareAssetTypeAgainst = type;
            if (typeToCompareAssetTypeAgainst.IsSubclassOf(typeof(Component)))
            {
                typeToCompareAssetTypeAgainst = typeof(GameObject);
            }
            //Work off GUIDS, like god intended
            foreach (var guid in AddressablesPathDictionary.instance.GetAllGUIDS())
            {
                Type assetType = GetAssetType(guid);
                //If we obtained the resource type, and the resource type is same or subclass of type, add it to the result
                if (assetType != null && (assetType == typeToCompareAssetTypeAgainst || assetType.IsSubclassOf(typeToCompareAssetTypeAgainst)))
                {
                    //We need to do an extra check if the type to compare against is GameObject, and the actual type we want is a component.
                    if (typeToCompareAssetTypeAgainst == typeof(GameObject) && type.IsSubclassOf(typeof(Component)))
                    {
                        GameObject gameObject = Addressables.LoadAssetAsync<GameObject>(guid).WaitForCompletion();
                        if (!gameObject)
                        {
                            continue;
                        }

                        //If it doesnt have the component we want, skip it.
                        bool hasComponent = searchInChildren ? gameObject.GetComponentInChildren(type) : gameObject.TryGetComponent(type, out _);
                        if (!hasComponent)
                            continue;
                    }

                    resultGuids.Add(guid);
                    resultPaths.Add(AddressablesPathDictionary.instance.GetPathFromGUID(guid));
                }
            }

            CacheKey cacheKey = new CacheKey() { requiredType = (SerializableSystemType)type, entriesIncludeComponentFoundInChildren = searchInChildren };
            CacheHit cacheHit = new CacheHit();
            cacheHit.pathCache = resultPaths.ToArray();
            cacheHit.guidsCache = resultGuids.ToArray();

            //Save in the cache
            if (!cacheHit.isEmpty)
            {
                _typeResultCache.Add(cacheKey, cacheHit);
            }
            else
            {
                Debug.LogWarning($"Could not build cache for key {cacheKey}! No addressable assets fulfill the required type.");
            }

            return cacheHit;
        }

        private Type GetAssetType(string guid)
        {
            var resourceLocations = Addressables.LoadResourceLocationsAsync(guid).WaitForCompletion();

            //There's a chance that there's no resources at this guid... idk???
            if (resourceLocations.Count == 0)
            {
                //If this happens, return the type stored on the dictionary
                return AddressablesPathDictionary.instance.GetTypeFromGUID(guid);
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

        internal void OnBeforeSerialize()
        {
            _serializedTypeResultCache.Clear();
            foreach(var (cacheKey, cacheHit) in _typeResultCache)
            {
                _serializedTypeResultCache.Add(new SerializedTypeResultCache(cacheKey, cacheHit));
            }
        }

        internal void OnAfterSerialize()
        {
            _typeResultCache.Clear();
            for (int i = 0; i < _serializedTypeResultCache.Count; i++)
            {
                _typeResultCache[_serializedTypeResultCache[i].cacheKey] = _serializedTypeResultCache[i].cacheHit;
            }
        }
    }
}