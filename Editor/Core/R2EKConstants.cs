using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// Contains multiple constants related to R2EK
    /// </summary>
    public static class R2EKConstants
    {
        /// <summary>
        /// "RoR2EditorKit"
        /// </summary>
        public const string ROR2_EDITOR_KIT = "RoR2EditorKit";

        /// <summary>
        /// "Packages/riskofthunder-ror2editorkit"
        /// </summary>
        public const string PACKAGE_FOLDER_PATH = "Packages/" + PACKAGE_NAME;

        /// <summary>
        /// riskofthunder-ror2editorkit"
        /// </summary>
        public const string PACKAGE_NAME = "riskofthunder-ror2editorkit";

        /// <summary>
        /// "Assets/Create/RoR2EditorKit"
        /// </summary>
        public const string R2EK_CONTEXT_ROOT = "Assets/Create/" + ROR2_EDITOR_KIT;

        /// <summary>
        /// "Tools/RoR2EditorKit"
        /// </summary>
        public const string ROR2EK_MENU_ROOT = "Tools/" + ROR2_EDITOR_KIT;

        /// <summary>
        /// Contains multiple <see cref="AssetGUID{T}"/> for important assets that R2EK uses
        /// </summary>
        public static class AssetGUIDs
        {
            /// <summary>
            /// Reference to the TextAsset that tells R2EK that the R2EKScriptableGizmos are installed
            /// </summary>
            public static readonly AssetGUID<TextAsset> gizmosReadme = "69c8ad553a081794c9c0a6985bbb84d3";

            /// <summary>
            /// Reference to R2EK's Icon
            /// </summary>
            public static readonly AssetGUID<Texture> r2ekIcon = "62cd962c0bfb78f4b974cf3b550e754e";

            /// <summary>
            /// Reference to the Standing Template Body utilized by the CharacterBodyWizard
            /// </summary>
            public static readonly AssetGUID<GameObject> standingTemplateBody = "eb0611eeb0578544ea4dd28f74b66ba8";

            /// <summary>
            /// Reference to the Flying Template Body utilized by the CharacterBodyWizard
            /// </summary>
            public static readonly AssetGUID<GameObject> flyingTemplateBody = "bc5eaa02f0e93b04b95ba670fbdf2504";

            /// <summary>
            /// Reference to the Stationary Template Body utilized by the CharacterBodyWizard
            /// </summary>
            public static readonly AssetGUID<GameObject> stationaryTemplateBody = "ba7ba725a07707b4a912445ffceffc8c";

            /// <summary>
            /// Reference to the Boss Template Body utilized by the CharacterBodyWizard
            /// </summary>
            public static readonly AssetGUID<GameObject> bossTemplateBody = "7c254df95d6214e40a7a0198c27f7dad";

            /// <summary>
            /// Reference to the Stage Template utilized by the StageWizard
            /// </summary>
            public static readonly AssetGUID<SceneAsset> stageTemplate = "717669e0f1db24844bec177325679c34";

            /// <summary>
            /// Reference to the GUID of the ".unityPackage" that contains the RoR2ScriptableGizmos
            /// </summary>
            public static readonly string ror2IconsForScriptsGUID = "87985f697aee81b48922b790035e73e1";
        }

        /// <summary>
        /// Utility for getting common folder paths
        /// </summary>
        public static class FolderPaths
        {
            /// <summary>
            /// "Assets"
            /// </summary>
            private const string ASSETS = "Assets";

            /// <summary>
            /// "Packages"
            /// </summary>
            private const string PACKAGES = "Packages";

            /// <summary>
            /// "Library"
            /// </summary>
            private const string LIB = "Library";

            /// <summary>
            /// "ScriptAssemblies"
            /// </summary>
            private const string SCRIPT_ASSEMBLIES = "ScriptAssemblies";

            public static readonly string[] findAllFolders = new[] { PACKAGES, ASSETS };
            public static readonly string[] findAssetsFolders = new[] { ASSETS };
            public static readonly string[] findPackagesFolders = new[] { PACKAGES };

            /// <summary>
            /// Returns the Library folder of the project
            /// </summary>
            public static string libraryFolder
            {
                get
                {
                    var assetsPath = Application.dataPath;
                    var libFolder = assetsPath.Replace(ASSETS, LIB);
                    return libFolder;
                }
            }

            /// <summary>
            /// Returns the Script Assemblies folder of the project
            /// </summary>
            public static string scriptAssembliesFolder
            {
                get
                {
                    return System.IO.Path.Combine(libraryFolder, SCRIPT_ASSEMBLIES);
                }
            }
        }
    }
}