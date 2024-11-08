using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// Contains utility methods that interact with <see cref="AssetDatabase"/>
    /// </summary>
    public static class AssetDatabaseUtil
    {
        /// <summary>
        /// Directly obtains the GUID for the given asset in a hexadecimal representation, assuming the asset exists within the database
        /// </summary>
        /// <param name="asset">The asset to get it's guid</param>
        /// <returns>The asset's GUID in a hexadecimal representation, if the asset isnt within the AssetDatabase, it returns an empty string</returns>
        public static string GetAssetGUIDString(UnityEngine.Object asset)
        {
            if (!AssetDatabase.Contains(asset))
            {
                Debug.LogWarning($"Object {asset} is not an Asset in the AssetDatabase");
                return string.Empty;
            }
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
        }

        /// <summary>
        /// Directly obtains the GUID for the given asset, assuming the asset exists within the database
        /// </summary>
        /// <param name="asset">The asset to get it's guid</param>
        /// <returns>The guid for the asset, otherwise it returns an invalid GUID</returns>
        public static GUID GetAssetGUID(UnityEngine.Object asset)
        {
            return new GUID(GetAssetGUIDString(asset));
        }

        /// <summary>
        /// Loads a generic asset directly using it's GUID's hexadecimal representation
        /// </summary>
        /// <param name="guid">The guid of the asset</param>
        /// <param name="func">A function that can get called if the asset doesnt exist within the asset database</param>
        /// <returns>An asset, otherwise null.</returns>
        public static UnityEngine.Object LoadAssetFromGUID(string guid, Func<UnityEngine.Object> func = null)
        {
            return LoadAssetFromGUID<UnityEngine.Object>(new GUID(guid), func);
        }

        /// <summary>
        /// Loads a generic asset directly using it's GUID
        /// </summary>
        /// <param name="guid">The guid of the asset</param>
        /// <param name="func">A function that can get called if the asset doesnt exist within the asset database</param>
        /// <returns>An asset, otherwise null.</returns>
        public static UnityEngine.Object LoadAssetFromGUID(GUID guid, Func<UnityEngine.Object> func = null)
        {
            return LoadAssetFromGUID<UnityEngine.Object>(guid, func);
        }

        /// <summary>
        /// Loads a an asset of type <typeparamref name="T"/> directly using it's GUID's hexadecimal representation
        /// </summary>
        /// <typeparam name="T">The type of asset to load</typeparam>
        /// <param name="guid">The guid of the asset</param>
        /// <param name="defaultInitializer">A function that can get called if the asset doesnt exist within the asset database</param>
        /// <returns>An asset, otherwise null.</returns>
        public static T LoadAssetFromGUID<T>(string guid, Func<T> defaultInitializer = null) where T : UnityEngine.Object
        {
            return LoadAssetFromGUID(new GUID(guid), defaultInitializer);
        }

        /// <summary>
        /// Loads a an asset of type <typeparamref name="T"/> directly using it's GUID
        /// </summary>
        /// <typeparam name="T">The type of asset to load</typeparam>
        /// <param name="guid">The guid of the asset</param>
        /// <param name="defaultInitializer">A function that can get called if the asset doesnt exist within the asset database</param>
        /// <returns>An asset, otherwise null.</returns>
        public static T LoadAssetFromGUID<T>(GUID guid, Func<T> defaultInitializer = null) where T : UnityEngine.Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                return defaultInitializer?.Invoke() ?? default(T);
            }

            T obj = AssetDatabase.LoadAssetAtPath<T>(path);
            if (!obj)
            {
                return defaultInitializer?.Invoke() ?? default(T);
            }
            return obj;
        }

        /// <summary>
        /// Method that calls <see cref="AssetDatabase.FindAssets(string, string[])"/>, and returns the asset paths
        /// </summary>
        public static IEnumerable<string> FindAssetPaths(string filter, string[] searchFolders)
        {
            var guids = AssetDatabase.FindAssets(filter, searchFolders);
            return guids.Select(AssetDatabase.GUIDToAssetPath);
        }

        /// <summary>
        /// Finds all assets of Type T
        /// </summary>
        /// <typeparam name="T">The Type of asset to find</typeparam>
        /// <param name="assetNameFilter">A filter to narrow down the search results</param>
        /// <returns>An IEnumerable of all the Types found inside the AssetDatabase.</returns>
        public static IEnumerable<T> FindAssetsByType<T>(string assetNameFilter = null) where T : UnityEngine.Object
        {
            List<T> assets = new List<T>();
            string[] guids;
            if (assetNameFilter != null)
                guids = AssetDatabase.FindAssets($"{assetNameFilter} t:{typeof(T).Name}", null);
            else
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", null);

            return guids.Select(x => LoadAssetFromGUID<T>(x));
        }
    }
}