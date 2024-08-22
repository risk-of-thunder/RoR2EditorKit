using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace RoR2.Editor
{
    public struct AssetGUID<T> where T : UnityEngine.Object
    {
        public string guid;

        public static implicit operator bool(AssetGUID<T> asset)
        {
            var path = AssetDatabase.GUIDToAssetPath(asset.guid);
            return !string.IsNullOrEmpty(path);
        }

        public static implicit operator T(AssetGUID<T> asset)
        {
            return AssetDatabaseUtil.LoadAssetFromGUID<T>(asset.guid);
        }

        public static implicit operator AssetGUID<T>(string guid)
        {
            return new AssetGUID<T>
            {
                guid = guid,
            };
        }

        public static implicit operator AssetGUID<T>(T asset)
        {
            if(AssetDatabase.Contains(asset))
            {
                return new AssetGUID<T>
                {
                    guid = AssetDatabaseUtil.GetAssetGUID(asset),
                };
            }
            throw new ArgumentException($"{asset} is not part of the AssetDatabase and as such has no GUID", nameof(asset));
        }
    }
}