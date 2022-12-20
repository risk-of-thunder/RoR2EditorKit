using RoR2EditorKit.Common;
using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Manifests;
using ThunderKit.Core.Manifests.Datum;
using ThunderKit.Core.Manifests.Datums;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class R2APIMigrationWizard : CreatorWizardWindow
    {
        public MonoScript mainClass;
        public AssemblyDefinitionAsset assemblyDefinition;
        public Manifest modManifest;

        protected override string WizardTitleTooltip =>
@"The R2APIMigrationWizard is a custom wizard that's used to migrate mods that depend on R2API so they're updated to use the new SplitAssemblies update.
Does the following things:
1.- Replaces your MainClass' BepInDependency for R2API with multiple BepInDependencies using the detected r2api submodules. (For this to work properly, your mod must specify the dependency as this: ""[BepInDependency(""com.bepis.r2api"", BepInDependency.DependencyFlags.HardDependency)]"")
2.- Adds the detected r2api submodules to your AssemblyDefinition's precompiled assemblies array
3.- Adds the detected R2API submodules to your mod's manifest's Dependencies array";

        private List<Assembly> assemblyList = new List<Assembly>();
        private List<string> assemblyNames = new List<string>();
        private Type bepInDependencyType;

        [MenuItem(Constants.RoR2EditorKitScriptableRoot + "Wizards/R2API Migration", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<R2APIMigrationWizard>();
            window.Focus();
        }

        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();

            assemblyList = AppDomain.CurrentDomain.GetAssemblies().ToList();
            assemblyNames = assemblyList.Select(asm => asm.GetName().Name).ToList();
            bepInDependencyType = Type.GetType("BepInEx.BepInPlugin, BepInEx");
        }

        protected override async Task<bool> RunWizard()
        {
            if(!assemblyNames.Contains("BepInEx"))
            {
                Debug.LogError("Cannot update mod as BepInEx is not installed.");
                return false;
            }

            if(!assemblyNames.Contains("R2API.Core"))
            {
                Debug.LogError("Cannot update mod as R2API.Core is not installed.");
                return false;
            }

            try
            {
                await UpdateMainClass();
                await UpdateAssemblyDef();
                await UpdateManifest();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        private async Task UpdateMainClass()
        {
            if (!mainClass)
                return;

            var mainClassPath = AssetDatabase.GetAssetPath(mainClass);
            var fullPath = Path.GetFullPath(mainClassPath);

            string text = string.Empty;
            using (StreamReader reader = File.OpenText(fullPath))
            {
                text = await reader.ReadToEndAsync();
            }
            string dependencies = GetBepInDependencies();
            text = text.Replace($"[BepInDependency(\"com.bepis.r2api\", BepInDependency.DependencyFlags.HardDependency)]", dependencies);

            File.WriteAllText(fullPath, text, Encoding.UTF8);
        }

        private string GetBepInDependencies()
        {
            StringBuilder stringBuilder = new StringBuilder();
            Assembly[] r2apiAssemblies = assemblyList.Where(asm => asm.GetName().Name.StartsWith("R2API.")).ToArray();

            foreach (Assembly asm in r2apiAssemblies)
            {
                if (TryGetModGUID(asm, out string guid))
                {
                    stringBuilder.Append($"[BepInDependency(\"{guid}\")]\n");
                }
            }

            return stringBuilder.ToString();
        }

        private bool TryGetModGUID(Assembly asm, out string guid)
        {
            Type mainClass = asm.GetTypesSafe().Where(t => t.GetCustomAttribute(bepInDependencyType) != null).FirstOrDefault();
            if (mainClass == null)
            {
                guid = null;
                return false;
            }

            Attribute bepInPluginAttribute = mainClass.GetCustomAttribute(bepInDependencyType);
            if (bepInPluginAttribute == null)
            {
                guid = null;
                return false;
            }

            Type attributeType = bepInPluginAttribute.GetType();

            PropertyInfo propInfo = attributeType.GetProperty("GUID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            if (propInfo == null)
            {
                guid = null;
                return false;
            }

            MethodInfo method = propInfo.GetMethod;
            if (method == null)
            {
                guid = null;
                return false;
            }

            guid = (string)method.Invoke(bepInPluginAttribute, null);
            return true;
        }

        private Task UpdateAssemblyDef()
        {
            if(!assemblyDefinition)
            {
                return Task.CompletedTask;
            }

            var def = JsonUtility.FromJson<ThunderKit.Core.Data.AssemblyDef>(assemblyDefinition.text);

            List<string> precompiledReferences = def.precompiledReferences.ToList();

            if(precompiledReferences.Contains("R2API.dll"))
            {
                precompiledReferences.Remove("R2API.dll");
            }

            var filteredNames = assemblyNames.Where(asm => asm.StartsWith("R2API.")).Select(asm => $"{asm}.dll");
            precompiledReferences.AddRange(filteredNames);
            def.precompiledReferences = precompiledReferences.ToArray();

            var assetFullPath = Path.GetFullPath(AssetDatabase.GetAssetPath(assemblyDefinition));
            File.WriteAllText(assetFullPath, JsonUtility.ToJson(def, true), Encoding.UTF8);
            return Task.CompletedTask;
        }

        private Task UpdateManifest()
        {
            if (!modManifest)
                return Task.CompletedTask;

            List<Manifest> dependencies = modManifest.Identity.Dependencies.Where(dep => dep).ToList();
            List<string> dependenciesAsNames = dependencies.Select(dep => dep.name).ToList();
            if(dependenciesAsNames.Contains("R2API"))
            {
                int index = dependenciesAsNames.IndexOf("R2API");
                dependencies.RemoveAt(index);
            }

            List<Manifest> manifests = AssetDatabaseUtils.FindAssetsByType<Manifest>().ToList();
            foreach (Manifest manifest in manifests)
            {
                if (manifest.name.StartsWith("R2API_"))
                {
                    dependencies.Add(manifest);
                }
            }
            modManifest.Identity.Dependencies = dependencies.ToArray();
            EditorUtility.SetDirty(modManifest);

            return Task.CompletedTask;
        }
    }
}