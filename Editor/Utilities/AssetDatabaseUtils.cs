using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Utilities
{
    public static class AssetDatabaseUtils
    {
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
            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                    assets.Add(asset);
            }
            return assets;
        }

        /// <summary>
        /// Finds an asset of Type T
        /// </summary>
        /// <typeparam name="T">The Type of asset to find</typeparam>
        /// <param name="assetNameFilter">A filter to narrow down the search results</param>
        /// <returns>The asset found</returns>
        public static T FindAssetByType<T>(string assetNameFilter = null) where T : UnityEngine.Object
        {
            string[] guids;

            if (assetNameFilter != null)
                guids = AssetDatabase.FindAssets($"{assetNameFilter} t{typeof(T).Name}", null);
            else
                guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", null);

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids.First()));
        }

        /// <summary>
        /// Creates a generic asset at the currently selected folder
        /// </summary>
        /// <param name="asset">The asset to create</param>
        /// <returns>The Created asset</returns>
        public static Object CreateAssetAtSelectionPath(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/{asset.name}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.ImportAsset(path);
            AssetDatabase.SaveAssets();

            return asset;
        }

        /// <summary>
        /// Creates a prefab at the currently selected folder
        /// </summary>
        /// <param name="asset">The prefab to create</param>
        /// <returns>The newely created prefab in the AssetDatabase</returns>
        public static GameObject CreatePrefabAtSelectionPath(GameObject asset)
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/{asset.name}.prefab");
            return PrefabUtility.SaveAsPrefabAsset(asset, path);
        }

        /// <summary>
        /// Updates the assetName of <paramref name="obj"/> so it displays properly
        /// </summary>
        /// <param name="obj">The object to update</param>
        public static void UpdateNameOfObject(Object obj)
        {
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), obj.name);
        }

        /// <summary>
        /// Loads an asset of type T by using it's internal GUID
        /// </summary>
        /// <typeparam name="T">The type of asset to load</typeparam>
        /// <param name="guid">The guid of the asset</param>
        /// <returns>The loaded object, null if the object does not exist in the asset database</returns>
        public static T LoadAssetFromGUID<T>(string guid) where T : Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            return path.IsNullOrEmptyOrWhitespace() ? null : AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// Retrieves the GUID of an asset
        /// </summary>
        /// <param name="obj">The asset to get the guid from</param>
        /// <returns>The GUID of the asset, if the asset does not exist in the database, it returns an empty string</returns>
        public static string GetGUIDFromAsset(Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            return path.IsNullOrEmptyOrWhitespace() ? string.Empty : AssetDatabase.AssetPathToGUID(path);
        }
    }
}
