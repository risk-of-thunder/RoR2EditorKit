using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Utilities;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Path = System.IO.Path;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public sealed class StageCreatorWizard : CreatorWizardWindow
    {
        public string stageName;
        public int stageOrder;

        protected override string WizardTitleTooltip =>
@"The StageCreatorWizard is a Custom Wizard that creates the following upon completion:
1.- A basic Scene asset with the required components to be used as a Stage in a run
2.- A SceneDef that points towards the  SceneAsset";
        protected override bool RequiresTokenPrefix => true;

        private string upper;
        private string lower;

        [MenuItem(Constants.RoR2EditorKitScriptableRoot + "Wizards/Stage", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<StageCreatorWizard>();
            window.Focus();
        }

        protected override async Task<bool> RunWizard()
        {
            if (stageName.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("stageName is null, empty or whitespace.");
                return false;
            }

            if (Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("tokenPrefix is null, empty or whitespace");
                return false;
            }

            try
            {
                upper = Settings.GetPrefixUppercase();
                lower = Settings.GetPrefixLowercase();
                await DuplicateSceneAsset();
                await CreateSceneDef();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        private Task DuplicateSceneAsset()
        {
            var path = IOUtils.GetCurrentDirectory();
            var sceneAsset = Constants.AssetGUIDS.QuickLoad<SceneAsset>(Constants.AssetGUIDS.stageTemplateGUID);
            var destPath = IOUtils.FormatPathForUnity(Path.Combine(path, $"{lower}_{stageName}.unity"));
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

            var tokenBase = $"{upper}_MAP_{stageName.ToUpperInvariant()}_";
            sceneDef.nameToken = $"{tokenBase}NAME";
            sceneDef.subtitleToken = $"{tokenBase}SUBTITLE";

            sceneDef.portalSelectionMessageString = $"{upper}_BAZAAR_SEER_{stageName.ToUpperInvariant()}";

            sceneDef.shouldIncludeInLogbook = true;
            sceneDef.loreToken = $"{tokenBase}LORE";

            sceneDef.validForRandomSelection = true;

            var directory = IOUtils.GetCurrentDirectory();
            var projectRelativePath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(directory));

            AssetDatabase.CreateAsset(sceneDef, $"{projectRelativePath}/{lower}_{stageName}.asset");
            return Task.CompletedTask;
        }
    }
}
