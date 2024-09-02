using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    public static class R2EKConstants
    {
        public const string ROR2_EDITOR_KIT = "RoR2EditorKit";
        public const string PACKAGE_FOLDER_PATH = "Packages/riskofthunder-ror2editorkit";
        public const string PACKAGE_NAME = "riskofthunder-ror2editorkit";

        public const string R2EK_CONTEXT_ROOT = "Assets/Create/RoR2EditorKit";
        public const string ROR2EK_MENU_ROOT = "Tools/RoR2EditorKit";

        public static class AssetGUIDs
        {
            public static readonly AssetGUID<TextAsset> gizmosReadme = "69c8ad553a081794c9c0a6985bbb84d3";
            public static readonly AssetGUID<Texture> r2ekIcon = "62cd962c0bfb78f4b974cf3b550e754e";
            public static readonly AssetGUID<GameObject> standingTemplateBody = "eb0611eeb0578544ea4dd28f74b66ba8";
            public static readonly AssetGUID<GameObject> flyingTemplateBody = "bc5eaa02f0e93b04b95ba670fbdf2504";
            public static readonly AssetGUID<GameObject> stationaryTemplateBody = "ba7ba725a07707b4a912445ffceffc8c";
            public static readonly AssetGUID<GameObject> bossTemplateBody = "7c254df95d6214e40a7a0198c27f7dad";
            public static readonly AssetGUID<SceneAsset> stageTemplate = "717669e0f1db24844bec177325679c34";

            public static readonly string ror2IconsForScriptsGUID = "87985f697aee81b48922b790035e73e1";
        }

        public static class FolderPaths
        {
            private const string ASSETS = "Assets";
            private const string PACKAGES = "Packages";
            private const string LIB = "Library";
            private const string SCRIPT_ASSEMBLIES = "ScriptAssemblies";

            public static readonly string[] findAllFolders = new[] { PACKAGES, ASSETS };
            public static readonly string[] findAssetsFolders = new[] { ASSETS };
            public static readonly string[] findPackagesFolders = new[] { PACKAGES };
            public static string libraryFolder
            {
                get
                {
                    var assetsPath = Application.dataPath;
                    var libFolder = assetsPath.Replace(ASSETS, LIB);
                    return libFolder;
                }
            }

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