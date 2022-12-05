using HG;
using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Path = System.IO.Path;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public sealed class CharacterBodyCreatorWizard : CreatorWizardWindow
    {
        public string characterName;
        [SerializableSystemType.RequiredBaseType(typeof(MonoBehaviour))]
        public List<SerializableSystemType> extraComponents = new List<SerializableSystemType>();
        public GameObject modelFBX;

        public enum TemplateOptions
        {
            Grounded,
            Flying,
            Stationary,
            Boss
        }

        private Dictionary<TemplateOptions, string> TemplateIndex = new Dictionary<TemplateOptions, string>
        {
            {TemplateOptions.Grounded, Constants.AssetGUIDS.groundedCharacterBodyTemplateGUID },
            {TemplateOptions.Flying, Constants.AssetGUIDS.flyingCharacterBodyTemplateGUID},
            {TemplateOptions.Stationary, Constants.AssetGUIDS.stationaryCharacterBodyTemplateGUID },
            {TemplateOptions.Boss, Constants.AssetGUIDS.bossCharacterBodyTemplateGUID }
        };

        public TemplateOptions bodyType;

        protected override string WizardTitleTooltip =>
@"The CharacterBodyCreatorWizard is a custom wizard that creates the following upon completion:
1.- A Prefab of a CharacterBody with filled tokens, necesary components, a minimum of 3 EntityStatemachines and a minimum of 4 Generic Skills, alongside the instantiated model.
(Note: The prefab itself is based off a stripped down version of the Commando's body prefab)";
        protected override bool RequiresTokenPrefix => true;

        private GameObject copiedBody;

        [MenuItem(Constants.RoR2EditorKitScriptableRoot + "Wizards/CharacterBody", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<CharacterBodyCreatorWizard>();
            window.Focus();
        }

        protected override async Task<bool> RunWizard()
        {
            if (characterName.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("characterName is null, empty or whitespace!");
                return false;
            }

            try
            {
                await InstantiateAndUnpackPrefab();
                await SetPrefabNameAndTokens();
                await AddExtraComponents();
                await SetupModelGameObject();
                await MakeIntoPrefabAsset();
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
            DestroyImmediate(copiedBody);
        }
        private Task InstantiateAndUnpackPrefab()
        {
            string templateToLoad = TemplateIndex[bodyType];
            var prefab = Constants.AssetGUIDS.QuickLoad<GameObject>(templateToLoad);
            copiedBody = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            return Task.CompletedTask;
        }

        private Task SetPrefabNameAndTokens()
        {
            CharacterBody body = copiedBody.GetComponent<CharacterBody>();
            string templateToken = $"{Settings.GetPrefixUppercase()}_{characterName.ToUpperInvariant()}_BODY_";
            body.baseNameToken = templateToken + "NAME";
            body.subtitleNameToken = templateToken + "SUBTITLE";

            copiedBody.name = $"{characterName}Body";

            return Task.CompletedTask;
        }

        private Task AddExtraComponents()
        {
            foreach (SerializableSystemType type in extraComponents)
            {
                try
                {
                    Type t = (Type)type;
                    copiedBody.AddComponent(t);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            return Task.CompletedTask;
        }

        private Task SetupModelGameObject()
        {
            GameObject modelBase = copiedBody.transform.Find("ModelBase").gameObject;
            GameObject templateMdl = modelBase.transform.GetChild(0).gameObject;
            GameObject mdlInstance = Instantiate(modelFBX, modelBase.transform);

            foreach (Component component in templateMdl.GetComponents<MonoBehaviour>())
            {
                try
                {
                    Component newComponent = mdlInstance.AddComponent(component.GetType());
                    foreach (FieldInfo f in component.GetType().GetFields())
                    {
                        f.SetValue(newComponent, f.GetValue(component));
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to copy component {component.GetType().Name} \n\n{e}");
                }
            }
            mdlInstance.name = $"mdl{characterName}";
            DestroyImmediate(templateMdl);
            return Task.CompletedTask;
        }

        private Task MakeIntoPrefabAsset()
        {
            var path = IOUtils.GetCurrentDirectory();
            var destPath = IOUtils.FormatPathForUnity(Path.Combine(path, $"{characterName}Body.prefab"));
            var projectRelative = FileUtil.GetProjectRelativePath(destPath);
            PrefabUtility.SaveAsPrefabAsset(copiedBody, projectRelative);
            AssetDatabase.ImportAsset(projectRelative);
            return Task.CompletedTask;
        }
    }
}
