using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    public static class AssetDatabaseUtil
    {
        public static string GetAssetGUID(UnityEngine.Object asset)
        {
            if(!AssetDatabase.Contains(asset))
            {
                Debug.LogWarning($"Object {asset} is not an Asset in the AssetDatabase");
                return string.Empty;
            }
            return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(asset));
        }
        public static UnityEngine.Object LoadAssetFromGUID(string guid, Func<UnityEngine.Object> func = null)
        {
            return LoadAssetFromGUID<UnityEngine.Object>(guid, func);
        }

        public static T LoadAssetFromGUID<T>(string guid, Func<T> defaultInitializer = null) where T : UnityEngine.Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if(string.IsNullOrEmpty(path))
            {
                Debug.LogWarning($"Cannot load asset with guid {guid} as the AssetDatabase doesnt contain said guid.");
                return defaultInitializer?.Invoke() ?? default(T);
            }

            T obj = AssetDatabase.LoadAssetAtPath<T>(path);
            if(!obj)
            {
                Debug.LogWarning($"Cannot load asset with guid {guid} using type {typeof(T).Name}.");
                return defaultInitializer?.Invoke() ?? default(T);
            }
            return obj;
        }

        public static IEnumerable<string> FindAssetPaths(string filter, string[] searchFolders)
        {
            var guids = AssetDatabase.FindAssets(filter, searchFolders);
            return guids.Select(AssetDatabase.GUIDToAssetPath);
        }
    }
}