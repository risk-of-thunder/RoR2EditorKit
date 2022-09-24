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
    public sealed class SurvivorCreatorWizard : CreatorWizardWindow
    {
        public CharacterBody characterBody;
        public float sortPosition;

        protected override string WizardTitleTooltip =>
@"The SurvivorCreatorWizard is a custom wizard that creates the following upon completion:
1.- A SurvivorDef for the specified CharacterBody with filled tokens.
2.- The SurvivorDef's DisplayModel, created from the CharacterBody's display model
If you dont have a CharacterBody for this wizard, use the CharacterBodyWizard to generate one.";
        protected override bool RequiresTokenPrefix => true;

        private GameObject instantiatedBody;
        private GameObject instantiatedModel;
        private GameObject finalizedDisplayPrefab;
        private string characterName;

        [MenuItem(Constants.RoR2EditorKitScriptableRoot + "Wizards/Survivor", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<SurvivorCreatorWizard>();
            window.Focus();
        }

        protected override async Task<bool> RunWizard()
        {
            if (Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("TokenPRefix is null, empty or whitespace");
                return false;
            }
            if (characterBody == null)
            {
                Debug.LogError("No CharacterBody supplied.");
                return false;
            }

            if (!characterBody.GetComponentInChildren<CharacterModel>())
            {
                Debug.LogError("The provided CharacterBody does not have a CharacterModel component in its hierarchy");
                return false;
            }

            characterName = characterBody.gameObject.name.Replace("Body", string.Empty);

            try
            {
                await CreateModelDisplay();
                await CleanModelDisplayOfUnecesaryComponents();
                await MakeDisplayModelIntoPrefab();
                await CreateSurvivorDef();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        protected override void Cleanup()
        {
            DestroyImmediate(instantiatedModel);
            DestroyImmediate(instantiatedBody);
        }

        private Task CreateModelDisplay()
        {
            instantiatedBody = Instantiate(characterBody.gameObject, Vector3.zero, Quaternion.identity);
            instantiatedModel = instantiatedBody.GetComponentInChildren<CharacterModel>().gameObject;
            instantiatedModel.transform.parent = null;
            return Task.CompletedTask;
        }

        private Task CleanModelDisplayOfUnecesaryComponents()
        {
            Component[] components = instantiatedModel.GetComponents<MonoBehaviour>();
            foreach (Component component in components)
            {
                var componentName = component.GetType().Name;
                if (componentName == "Transform" || componentName == "CharacterModel" || componentName == "Animator")
                    continue;

                DestroyImmediate(component);
            }
            return Task.CompletedTask;
        }

        private Task MakeDisplayModelIntoPrefab()
        {
            var path = IOUtils.GetCurrentDirectory();
            var destPath = IOUtils.FormatPathForUnity(Path.Combine(path, $"{characterName}Display.prefab"));
            var projectRelative = FileUtil.GetProjectRelativePath(destPath);
            finalizedDisplayPrefab = PrefabUtility.SaveAsPrefabAsset(instantiatedModel, projectRelative);
            AssetDatabase.ImportAsset(projectRelative);
            return Task.CompletedTask;
        }

        private Task CreateSurvivorDef()
        {
            var survivorDef = CreateInstance<SurvivorDef>();

            survivorDef.bodyPrefab = characterBody.gameObject;
            survivorDef.displayPrefab = finalizedDisplayPrefab;
            survivorDef.hidden = false;
            survivorDef.desiredSortPosition = sortPosition;
            survivorDef.primaryColor = characterBody.bodyColor;

            string tokenBase = $"{Settings.GetPrefixUppercase()}_{characterName.ToUpperInvariant()}_";
            survivorDef.displayNameToken = tokenBase + "BODY_NAME";
            survivorDef.descriptionToken = tokenBase + "DESCRIPTION";
            survivorDef.outroFlavorToken = tokenBase + "OUTRO_FLAVOR";
            survivorDef.mainEndingEscapeFailureFlavorToken = tokenBase + "MAIN_ENDING_ESCAPE_FAILURE_FLAVOR";

            var directory = IOUtils.GetCurrentDirectory();
            var destPath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(Path.Combine(directory, $"{characterName}.asset")));

            AssetDatabase.CreateAsset(survivorDef, destPath);
            return Task.CompletedTask;
        }
    }
}