using UnityEditor;
using UnityEngine;
using System.IO;

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

        private const string nullMaterialGUID = "732339a737ef9a144812666d298e2357";
        private const string nullMeshGUID = "9bef9cd9cd0c4b244ad1ff166c26f57e";
        private const string nullSpriteGUID = "1a8e7e70058f32f4483753ec5be3838b";
        private const string nullPrefabGUID = "f6317a68216520848aaef2c2f470c8b2";
        private const string iconGUID = "efa2e3ecb36780a4d81685ecd4789ff3";
        private const string xmlDocGUID = "c78bcabe3d7e88545a1fbf97410ae546";
        private const string mainClassTemplate = "7eb4c9a0028b715499bc7919670b7098";
        private const string stageTemplate = "cadab9e52b34ebe45bad66b94b3b1cff";

        /// <summary>
        /// Loads the RoR2EditorKit null material
        /// </summary>
        public static Material NullMaterial => Load<Material>(nullMaterialGUID);

        /// <summary>
        /// Loads the RoR2EditorKit null mesh
        /// </summary>
        public static Mesh NullMesh => Load<Mesh>(nullMeshGUID);

        /// <summary>
        /// Loads the RoR2EditorKit null sprite
        /// </summary>
        public static Sprite NullSprite => Load<Sprite>(nullSpriteGUID);

        /// <summary>
        /// Loads the RoR2EditorKit null prefab
        /// </summary>
        public static GameObject NullPrefab => Load<GameObject>(nullPrefabGUID);

        /// <summary>
        /// Loads the RoR2EditorKit icon
        /// </summary>
        public static Texture Icon => Load<Texture>(iconGUID);

        /// <summary>
        /// Loads the XMLDoc of RoR2EditorKit
        /// </summary>
        public static TextAsset XMLDoc => Load<TextAsset>(xmlDocGUID);

        /// <summary>
        /// Loads the main class template
        /// </summary>
        public static TextAsset MainClassTemplate => Load<TextAsset>(mainClassTemplate);

        /// <summary>
        /// Loads the stage template
        /// </summary>
        public static SceneAsset StageTemplate => Load<SceneAsset>(stageTemplate);

        private static T Load<T>(string guid) where T : UnityEngine.Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }

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
