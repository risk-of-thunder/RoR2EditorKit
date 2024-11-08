using System;
using UnityEditor;

namespace RoR2.Editor
{
    /// <summary>
    /// An <see cref="AssetGUID{T}"/> is a way to keep references to assets in code, but instead of directly referencing them it references their GUID's, useful for keeping constants in other editor related packages.
    /// 
    /// <para>It also contains implicit operators for casting purposes, such as casting to bool, <typeparamref name="T"/>, or encapsulating a string as an <see cref="AssetGUID{T}"/></para>
    /// </summary>
    /// <typeparam name="T">The type of asset this AssetGUID loads</typeparam>
    public struct AssetGUID<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// The GUID of the Asset
        /// </summary>
        public GUID guid;

        /// <summary>
        /// Returns true if <paramref name="asset"/>'s <see cref="guid"/> returns a non empty string
        /// </summary>
        public static implicit operator bool(AssetGUID<T> asset)
        {
            var path = AssetDatabase.GUIDToAssetPath(asset.guid);
            return !string.IsNullOrEmpty(path);
        }

        /// <summary>
        /// Returns the asset that this <see cref="AssetGUID{T}"/> references
        /// </summary>
        public static implicit operator T(AssetGUID<T> asset)
        {
            return AssetDatabaseUtil.LoadAssetFromGUID<T>(asset.guid);
        }

        /// <summary>
        /// Encapsulates a GUID as an <see cref="AssetGUID{T}"/>
        /// </summary>
        public static implicit operator AssetGUID<T>(GUID guid)
        {
            return new AssetGUID<T>
            {
                guid = guid
            };
        }

        /// <summary>
        /// Encapsulates a hexadecimal GUID as an <see cref="AssetGUID{T}"/>
        /// </summary>
        public static implicit operator AssetGUID<T>(string guid)
        {
            return new GUID(guid);
        }

        /// <summary>
        /// Encapsulates an asset as an <see cref="AssetGUID{T}"/>
        /// <para>Throws an exception is <paramref name="asset"/> is not within the asset database.</para>
        /// </summary>
        /// <param name="asset"></param>
        public static implicit operator AssetGUID<T>(T asset)
        {
            if (AssetDatabase.Contains(asset))
            {
                return new AssetGUID<T>
                {
                    guid = AssetDatabaseUtil.GetAssetGUID(asset),
                };
            }
            throw new ArgumentException($"{asset} is not part of the AssetDatabase and as such has no GUID", nameof(asset));
        }

        /// <summary>
        /// Returns the AssetPath of this AssetGUID
        /// 
        /// <para>See also <see cref="ToString(bool)"/></para>
        /// </summary>
        /// <returns>The AssetPath of this AssetGUID</returns>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Custom ToString option that can be used to either return the <see cref="guid"/> in its hexadecimal representation, or to return the AssetPath for the guid.
        /// </summary>
        /// <param name="returnGUIDAsString">If true, the GUID is returned using its hexadecimal representation, otherwise the asset path is returned</param>
        /// <returns>If <paramref name="returnGUIDAsString"/> is true, the GUID is returned using its hexadecimal representation, otherwise the asset path is returned</returns>
        public string ToString(bool returnGUIDAsString)
        {
            return returnGUIDAsString ? guid.ToString() : AssetDatabase.GUIDToAssetPath(guid);
        }
    }
}