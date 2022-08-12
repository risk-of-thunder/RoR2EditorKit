using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Settings;
using RoR2EditorKit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEngine;
using Path = System.IO.Path;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class StageCreatorWizard : CreatorWizardWindow
    {
        public string stageName;
        public int stageOrder;

        [MenuItem(Constants.RoR2EditorKitScriptableRoot + "Wizards/Stage", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<StageCreatorWizard>();
            window.Focus();
        }

        protected override async Task<bool> RunWizard()
        {
            if(stageName.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("stageName is null, empty or whitespace.");
                return false;
            }

            if(Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("tokenPrefix is null, empty or whitespace");
                return false;
            }

            try
            {
                await DuplicateSceneAsset();
                await CreateSceneDef();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        private Task DuplicateSceneAsset()
        {
            var path = IOUtils.GetCurrentDirectory();
            var sceneAsset = Constants.StageTemplate;
            var destPath = IOUtils.FormatPathForUnity(Path.Combine(path, $"{stageName}.unity"));
            var sceneAssetPath = AssetDatabase.GetAssetPath(sceneAsset);
            AssetDatabase.CopyAsset(sceneAssetPath, FileUtil.GetProjectRelativePath(destPath));
            return Task.CompletedTask;
        }

        private Task CreateSceneDef()
        {
            var sceneDef = ScriptableObject.CreateInstance<SceneDef>();

            sceneDef.baseSceneNameOverride = stageName;
            sceneDef.sceneType = SceneType.Stage;
            sceneDef.stageOrder = stageOrder;

            var tokenBase = $"{Settings.GetPrefixUppercase()}_MAP_{stageName.ToUpperInvariant()}_";
            sceneDef.nameToken = $"{tokenBase}NAME";
            sceneDef.subtitleToken = $"{tokenBase}SUBTITLE";

            sceneDef.portalSelectionMessageString = $"{Settings.GetPrefixUppercase()}_BAZAAR_SEER_{stageName.ToUpperInvariant()}";

            sceneDef.shouldIncludeInLogbook = true;
            sceneDef.loreToken = $"{tokenBase}LORE";

            sceneDef.validForRandomSelection = true;

            var directory = IOUtils.GetCurrentDirectory();
            var projectRelativePath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(directory));

            AssetDatabase.CreateAsset(sceneDef, $"{projectRelativePath}/{stageName}.asset");
            return Task.CompletedTask;
        }
    }
}
