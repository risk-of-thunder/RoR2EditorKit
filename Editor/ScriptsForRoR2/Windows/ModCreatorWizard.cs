using RoR2EditorKit.Common;
using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Manifests.Datums;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public sealed class ModCreatorWizard : CreatorWizardWindow
    {
        public string authorName;
        public string modName;
        public string humanReadableModName;
        public string modDescription;

        protected override string WizardTitleTooltip =>
@"The ModCreatorWizard is a custom wizard that creates the following upon completion:
1.- An AssemblyDef with references to most common ror2 modding assemblies
2.- A very basic MainClass following a basic MonoBehaviour Singleton pattern
3.- A folder for your Assets for the AssetBundle
4.- A ThunderKit Manifest for your mod.";

        private List<string> assemblyList = new List<string>();
        private UnityEngine.Object assetBundleFolder;
        private AssemblyDefinitionAsset assemblyDef;

        [MenuItem(Constants.RoR2EditorKitScriptableRoot + "Wizards/Mod", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<ModCreatorWizard>();
            window.Focus();
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            var textField = WizardElementContainer.Q<TextField>("modDescription");
            modDescription = textField.text;
        }

        protected override async Task<bool> RunWizard()
        {
            if (authorName.IsNullOrEmptyOrWhitespace() || modName.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("authorName or modName is null, empty or whitespace!");
                return false;
            }

            assemblyList = AppDomain.CurrentDomain.GetAssemblies().Select(asm => asm.GetName().Name).ToList();
            if (!assemblyList.Contains("BepInEx"))
            {
                Debug.LogError("Cannot build mod from wizard since BepInEx is not installed.");
                return false;
            }

            try
            {
                await CreateAssemblyDef();
                await CreateMainClass();
                await CreateAssetbundleFolder();
                await CreateManifest();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        private async Task CreateAssemblyDef()
        {
            var def = new ThunderKit.Core.Data.AssemblyDef();
            def.name = modName;
            def.references = new string[]
            {
                "Unity.Postprocessing.Runtime",
                "com.unity.multiplayer-hlapi.Runtime",
                "Unity.TextMeshPro",
                "UnityEngine.UI"
            };
            def.overrideReferences = true;
            def.precompiledReferences = new string[]
            {
                "BepInEx.dll",
                "R2API.dll",
                "MonoMod.Utils.dll",
                "Mono.Cecil.dll",
                "MMHOOK_RoR2.dll",
                "HGCSharpUtils.dll",
                "HGUnityUtils.dll",
                "Zio.dll",
                "RoR2.dll",
                "RoR2BepInExPack.dll",
                "Unity.Addressables.dll",
                "Unity.ResourceManager.dll",
                "Unity.TextMeshPro.dll",
                "UnityEngine.UI.dll",
                "Unity.Postprocessing.Runtime.dll"
            };
            def.autoReferenced = true;

            var directory = IOUtils.GetCurrentDirectory();
            string assemblyDefPath = Path.Combine(directory, $"{modName}.asmdef");

            using (var fs = File.CreateText(assemblyDefPath))
            {
                await fs.WriteAsync(EditorJsonUtility.ToJson(def, true));
            }

            var projectRelativePath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(assemblyDefPath));
            AssetDatabase.ImportAsset(projectRelativePath, ImportAssetOptions.Default);
            assemblyDef = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(projectRelativePath);
        }

        private async Task CreateMainClass()
        {
            var directory = IOUtils.GetCurrentDirectory();
            string mainClassPath = Path.Combine(directory, $"{modName}Main.cs");
            string mainClassTemplate = Constants.AssetGUIDS.QuickLoad<TextAsset>(Constants.AssetGUIDS.mainClassTemplateGUID).text;

            using (var fs = File.CreateText(mainClassPath))
            {
                string extraUsingClauses = string.Empty;
                string attributes = string.Empty;

                if (assemblyList.Contains("R2API"))
                {
                    extraUsingClauses = "using R2API;\n" +
                        "using R2API.ScriptableObjects;\n" +
                        "using R2API.Utils;\n" +
                        "using R2API.ContentManagement;";

                    attributes = "[BepInDependency(\"com.bepis.r2api\", BepInDependency.DependencyFlags.HardDependency)]";
                }

                await fs.WriteAsync(string.Format(mainClassTemplate, modName, humanReadableModName, authorName, extraUsingClauses, attributes));
            }

            var projectRelativePath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(mainClassPath));
            AssetDatabase.ImportAsset(projectRelativePath, ImportAssetOptions.Default);
        }

        private Task CreateAssetbundleFolder()
        {
            var directory = IOUtils.GetCurrentDirectory();
            var projectRelativePath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(directory));
            var guid = AssetDatabase.CreateFolder(projectRelativePath, $"{modName}Assets");
            assetBundleFolder = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(guid));
            return Task.CompletedTask;
        }

        private Task CreateManifest()
        {
            var directory = IOUtils.GetCurrentDirectory();
            var projectRelativePath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(directory));

            var newManifest = ScriptableObject.CreateInstance<Manifest>();
            AssetDatabase.CreateAsset(newManifest, $"{projectRelativePath}/{modName}Manifest.asset");

            newManifest.Identity = ScriptableObject.CreateInstance<ManifestIdentity>();
            var identity = newManifest.Identity;
            identity.name = nameof(ManifestIdentity);
            identity.Author = authorName;
            identity.Name = modName;
            identity.Description = modDescription;
            identity.Version = "1.0.0";
            newManifest.InsertElement(newManifest.Identity, 0);

            var bundleDatum = ScriptableObject.CreateInstance<AssetBundleDefinitions>();
            bundleDatum.assetBundles = new AssetBundleDefinition[1]
            {
                    new AssetBundleDefinition
                    {
                        assetBundleName = $"{modName}Assets",
                        assets = new UnityEngine.Object[] { assetBundleFolder }
                    }
            };

            newManifest.InsertElement(bundleDatum, 1);

            var assemblyDatum = ScriptableObject.CreateInstance<AssemblyDefinitions>();
            assemblyDatum.definitions = new UnityEditorInternal.AssemblyDefinitionAsset[1]
            {
                    assemblyDef
            };

            newManifest.InsertElement(assemblyDatum, 2);
            return Task.CompletedTask;
        }
    }
}