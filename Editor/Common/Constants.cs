using System.IO;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Common
{
    /// <summary>
    /// Class filled with constants to use for asset creation or attributes
    /// </summary>
    public static class Constants
    {
        public const string RoR2EditorKit = nameof(RoR2EditorKit);
        public const string AssetFolderPath = "Assets/RoR2EditorKit";
        public const string PackageFolderPath = "Packages/riskofthunder-ror2editorkit";
        public const string PackageName = "riskofthunder-ror2editorkit";

        public const string RoR2EditorKitContextRoot = "Assets/Create/RoR2EditorKit/";
        public const string RoR2EditorKitScriptableRoot = "Assets/RoR2EditorKit/";
        public const string RoR2EditorKitMenuRoot = "Tools/RoR2EditorKit/";
        public const string RoR2KitSettingsRoot = "Assets/ThunderkitSettings/RoR2EditorKit/";

        /// <summary>
        /// A collection of GUIDS that represent common assets in the ror2editorkit assets folder
        /// </summary>
        public static class AssetGUIDS
        {
            public const string nullMaterialGUID = "732339a737ef9a144812666d298e2357";
            public const string nullMeshGUID = "9bef9cd9cd0c4b244ad1ff166c26f57e";
            public const string nullSpriteGUID = "1a8e7e70058f32f4483753ec5be3838b";
            public const string nullPrefabGUID = "f6317a68216520848aaef2c2f470c8b2";
            public const string iconGUID = "efa2e3ecb36780a4d81685ecd4789ff3";
            public const string xmlDocGUID = "c78bcabe3d7e88545a1fbf97410ae546";
            public const string mainClassTemplateGUID = "7eb4c9a0028b715499bc7919670b7098";
            public const string stageTemplateGUID = "cadab9e52b34ebe45bad66b94b3b1cff";
            public const string groundedCharacterBodyTemplateGUID = "8cf61750955a3054c9153177612aa73f";
            public const string flyingCharacterBodyTemplateGUID = "1cfcc30cbe06ba04690bb7482452a310";
            public const string stationaryCharacterBodyTemplateGUID = "d19f25f365e24f843b17beab8edab99a";
            public const string bossCharacterBodyTemplateGUID = "f2d1c9974df5abb47b8f3f538d28ad11";
            /// <summary>
            /// Loads an asset of type <typeparamref name="T"/> by using the <paramref name="guid"/> provided
            /// </summary>
            /// <typeparam name="T">The type of asset to load</typeparam>
            /// <param name="guid">The guid to use</param>
            /// <returns>The loaded Asset</returns>
            public static T QuickLoad<T>(string guid) where T : UnityEngine.Object
            {
                return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
            }
            /// <summary>
            /// Transforms <paramref name="guid"/> into its asset path.
            /// </summary>
            /// <param name="guid">The asset's guid</param>
            /// <returns>The Asset's path</returns>
            public static string GetPath(string guid)
            {
                return AssetDatabase.GUIDToAssetPath(guid);
            }
        }

        /// <summary>
        /// Used to quickly get into folders from a unity project.
        /// </summary>
        public static class FolderPaths
        {
            private const string assets = "Assets";
            private const string lib = "Library";
            private const string scriptAssemblies = "ScriptAssemblies";
            public static string LibraryFolder
            {
                get
                {
                    var assetsPath = Application.dataPath;
                    var libFolder = assetsPath.Replace(assets, lib);
                    return libFolder;
                }
            }

            public static string ScriptAssembliesFolder
            {
                get
                {
                    return Path.Combine(LibraryFolder, scriptAssemblies);
                }
            }
        }

    }
}
