#if BBEPIS_BEPINEXPACK
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor.UIElements;
using System.IO;
using IOPath = System.IO.Path;
using RoR2.Editor.CodeGen;
using BepInEx;
using System.Reflection;
using System.Linq.Expressions;
using BepInEx.Logging;

namespace RoR2.Editor.Windows
{
    public class ModWizard : EditorWizardWindow
    {
        private static readonly AssetGUID<AssemblyDefinitionAsset>[] _defaultAssemblyDefinitions;
        private static readonly string[] _defaultPrecompiledAssemblies;
        private static string[] _r2apiPrecompiledAssemblies;

        [FilePickerPath(FilePickerPath.PickerType.OpenFolder, title = "Folder for created mod")]
        public string folderPath;

        public string authorName;
        public string modName;
        public string humanReadableModName;
        public string modDescription;
        public bool addAllR2APIAssemblies;
        public List<AssemblyDefinitionAsset> assemblyDefinitionReferences = new List<AssemblyDefinitionAsset>();
        public List<string> precompiledAssemblyReferences = new List<string>();

        protected override string wizardTitleTooltip =>
@"The ModWizard is a Wizard that creates a very basic BepInExMod from the data you provide.

The wizard will create an Assemblydef with references to your chosen Assembly References, a MainClass and a ContentProvider class that'll load your assetbundle asynchronously, a folder for your Assets that will go to the AssetBundle, and a ThunderKit manifest for your mod.";

        private WizardCoroutineHelper _wizardCoroutineHelper;

        private string _folderOutput;
        private string _assetBundleFolder;
        private AssemblyDefinitionAsset _modAssemblyDef;
        private string[] _allReferencedAssembliesFromModAssemblyDefinition;
        
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Wizards/Mod")]
        private static void OpenWindow() => Open<ModWizard>(null);

        protected override void SetupControls()
        {
            ListView listView = rootVisualElement.Q<ListView>();
            listView.makeItem = () => new IMGUIContainer();
            listView.bindItem = BindElement;
            listView.itemsSource = precompiledAssemblyReferences;
            listView.showBoundCollectionSize = false;
            listView.showBoundCollectionSize = true;

            rootVisualElement.Q<PropertyField>("AddAllR2APIAssemblies").SetDisplay(_r2apiPrecompiledAssemblies.Length != 0);
        }

        private void BindElement(VisualElement ve, int index)
        {
            IMGUIContainer dropdownField = (IMGUIContainer)ve;
            dropdownField.onGUIHandler = () =>
            {
                var rect = EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Assembly " + index, GUILayout.ExpandWidth(false));
                if(EditorGUILayout.DropdownButton(new GUIContent(precompiledAssemblyReferences[index], CompilationPipeline.GetPrecompiledAssemblyPathFromAssemblyName(precompiledAssemblyReferences[index])), FocusType.Passive))
                {
                    Rect labelRect = GUILayoutUtility.GetLastRect();

                    var rectToUse = new Rect
                    {
                        x = rect.x + labelRect.width,
                        y = rect.y,
                        height = rect.height,
                        width = rect.width - labelRect.width
                    };

                    var _dropdown = new PrecompiledAssemblyDropdown(null);

                    _dropdown.onItemSelected += (item) =>
                    {
                        precompiledAssemblyReferences[index] = item.name;
                    };
                    _dropdown.Show(rectToUse);
                }
                EditorGUILayout.EndHorizontal();
            };
        }

        protected override void Awake()
        {
            base.Awake();
            _r2apiPrecompiledAssemblies ??= CompilationPipeline.GetPrecompiledAssemblyNames().Where(s => s.StartsWith("R2API")).ToArray();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            foreach (var guid in _defaultAssemblyDefinitions)
            {
                if (AssetDatabase.GUIDToAssetPath(guid.guid).IsNullOrEmptyOrWhiteSpace())
                    continue;

                assemblyDefinitionReferences.Add(guid);
            }

            foreach(var assemblyName in _defaultPrecompiledAssemblies)
            {
                if (CompilationPipeline.GetPrecompiledAssemblyPathFromAssemblyName(assemblyName).IsNullOrEmptyOrWhiteSpace())
                    continue;

                precompiledAssemblyReferences.Add(assemblyName);
            }

            _wizardCoroutineHelper = new WizardCoroutineHelper(this);
            _wizardCoroutineHelper.AddStep(CreateFolders(), "Creating Folders");
            _wizardCoroutineHelper.AddStep(CreateAssemblyDef(), "Writing AssemblyDef");
            _wizardCoroutineHelper.AddStep(CreateMainClass(), "Writing Main Class");
            _wizardCoroutineHelper.AddStep(CreateContentProvider(), "Writing Content Provider");
        }

        protected override bool ValidateData()
        {
            if(authorName.IsNullOrEmptyOrWhiteSpace())
            {
                Debug.LogError($"Cannot run wizard, authorName is not valid.");
                return false;
            }

            if(modName.IsNullOrEmptyOrWhiteSpace())
            {
                Debug.LogError($"Cannot run wizard, modName is not valid");
                return false;
            }

            if(humanReadableModName.IsNullOrEmptyOrWhiteSpace())
            {
                Debug.LogError($"Cannot run wizard, humanReadableModName is not valid");
                return false;
            }

            if(modDescription.IsNullOrEmptyOrWhiteSpace())
            {
                Debug.LogError($"Cannot run wizard, modDescription is not valid");
                return false;
            }

            return true;
        }
        protected override IEnumerator RunWizardCoroutine()
        {
            EditorApplication.LockReloadAssemblies();
            try
            {
                while(_wizardCoroutineHelper.MoveNext())
                {
                    yield return _wizardCoroutineHelper.Current;
                }
            }
            finally
            {
                EditorApplication.UnlockReloadAssemblies();
            }
            yield break;
        }

        private IEnumerator CreateFolders()
        {
            _folderOutput = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(folderPath, modName));
            _assetBundleFolder = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(_folderOutput, "Assets"));
            yield return 1f;
            yield break;
        }

        private IEnumerator CreateAssemblyDef()
        {
            var def = new ThunderKit.Core.Data.AssemblyDef();
            def.name = modName;
            def.references = assemblyDefinitionReferences.Select(asmdef => $"GUID:" + AssetDatabaseUtil.GetAssetGUIDString(asmdef)).ToArray();
            def.overrideReferences = true;
            def.precompiledReferences = precompiledAssemblyReferences.ToArray();

            if (addAllR2APIAssemblies)
            {
                int num = 0;
                foreach (var asmName in _r2apiPrecompiledAssemblies)
                {
                    yield return R2EKMath.Remap(num, 0, _r2apiPrecompiledAssemblies.Length - 1, 0, 0.5f);
                    if (!def.precompiledReferences.Contains(asmName))
                    {
                        ArrayUtility.Add(ref def.precompiledReferences, asmName);
                    }
                    num++;
                }
            }

            def.autoReferenced = true;

            List<string> assemblyReferencesFromAssemblyDefinitionAssetReferences = def.references
                .Select(guid =>
                {
                    var substring = guid.Substring("GUID:".Length);
                    return AssetDatabaseUtil.LoadAssetFromGUID<AssemblyDefinitionAsset>(substring);
                })
                .Select(asset => JsonUtility.FromJson<ThunderKit.Core.Data.AssemblyDef>(asset.text))
                .Select(assemblyDef => assemblyDef.name)
                .ToList();
            _allReferencedAssembliesFromModAssemblyDefinition = assemblyReferencesFromAssemblyDefinitionAssetReferences.Concat(def.precompiledReferences.Select(IOPath.GetFileNameWithoutExtension)).ToArray();

            var directory = IOPath.GetFullPath(_folderOutput);
            var assemblyDefPath = IOPath.Combine(directory, $"{modName}.asmdef");

            yield return 0.5f;
            using (var fs = File.CreateText(assemblyDefPath))
            {
                var task = fs.WriteAsync(EditorJsonUtility.ToJson(def, true));
                while(!task.IsCompleted)
                {
                    yield return 0.75f;
                }
            }

            var projectRelativePath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(assemblyDefPath));
            AssetDatabase.ImportAsset(projectRelativePath);
            yield return 1f;
            _modAssemblyDef = AssetDatabase.LoadAssetAtPath<AssemblyDefinitionAsset>(projectRelativePath);
            yield break;
        }

        private IEnumerator CreateMainClass()
        {
            Writer writer = new Writer
            {
                buffer = new System.Text.StringBuilder()
            };
            var outputFileName = string.Empty;
            using(CodeGeneratorHelper helper = new CodeGeneratorHelper())
            {
                var className = modName + "Main";
                var fileName = className + ".cs";
                outputFileName = fileName;

                writer.WriteVerbatim(
@"using BepInEx;
using System.IO;
using UnityEngine;");
                
                writer.WriteLine($"namespace {helper.MakeIdentifierPascalCase(modName)}");
                writer.BeginBlock(); //namespace Begin

                writer.WriteLine("#region Dependencies");
                var subroutine = WriteBepInDependencies(writer);
                while(subroutine.MoveNext())
                {
                    yield return subroutine.Current;
                }
                writer.WriteLine("#endregion");

                writer.WriteLine($"[BepInPlugin(GUID, MODNAME, VERSION)]");
                writer.WriteLine($"public class {className} : {nameof(BepInEx.BaseUnityPlugin)}");
                writer.BeginBlock(); //class begin

                writer.WriteVerbatim(
@$"public const string GUID = ""com.{authorName}.{modName}"";
public const string MODNAME = ""{humanReadableModName}"";
public const string VERSION = ""0.0.1"";");

                writer.WriteLine();
                writer.WriteVerbatim(
@$"public static PluginInfo pluginInfo {{ get; private set; }}
public static {className} instance {{ get; private set; }}
internal static AssetBundle assetBundle {{ get; private set; }}
internal static string assetBundleDir => Path.Combine(Path.GetDirectoryName(pluginInfo.Location), ""assetbundles"");");

                writer.WriteLine();
                writer.WriteLine("private void Awake()");
                writer.BeginBlock(); //Awake begin

                writer.WriteVerbatim(
@$"instance = this;
pluginInfo = Info;
new {modName}Content();");
                

                writer.EndBlock(); //Awake end

                subroutine = WriteInternalStaticLogs(writer);
                while(subroutine.MoveNext())
                {
                    yield return subroutine.Current;
                }

                writer.EndBlock(); //class end
                writer.EndBlock(); //namespace end
            }

            var combinedPath = IOPath.Combine(_folderOutput, outputFileName);
            var validationData = new CodeGeneratorValidator.ValidationData
            {
                code = writer,
                desiredOutputPath = IOPath.GetFullPath(combinedPath)
            };
            var validationSubroutine = CodeGeneratorValidator.ValidateCoroutine(validationData);
            while(validationSubroutine.MoveNext())
            {
                yield return validationSubroutine.Current;
            }
            yield break;

            IEnumerator WriteBepInDependencies(Writer writer)
            {
                var baseUnityPlugins = TypeCache.GetTypesWithAttribute(typeof(BepInEx.BepInPlugin));
                int processedBUPS = 0; //Toad BUP sound
                foreach(Type baseUnityPlugin in baseUnityPlugins)
                {
                    yield return R2EKMath.Remap(processedBUPS, 0, baseUnityPlugins.Count, 0, 0.5f);

                    System.Reflection.Assembly assembly = baseUnityPlugin.Assembly;
                    var fileName = IOPath.GetFileNameWithoutExtension(assembly.Location);

                    //We're referencing this assembly, write bepindependency attribute.
                    if(_allReferencedAssembliesFromModAssemblyDefinition.Contains(fileName))
                    {
                        var bepInPlugin = baseUnityPlugin.GetCustomAttribute<BepInPlugin>();
                        writer.WriteLine($"[BepInDependency(\"{bepInPlugin.GUID}\")]");
                    }

                    processedBUPS++;
                }
                yield break;
            }

            IEnumerator WriteInternalStaticLogs(Writer writer)
            {
                Array enumValues = Enum.GetValues(typeof(LogLevel));
                int enumValuesProcessed = 0;
                foreach(LogLevel enumValue in Enum.GetValues(typeof(LogLevel)))
                {
                    yield return R2EKMath.Remap(enumValuesProcessed, 0, enumValues.Length, 0.5f, 1f);

                    if (enumValue == LogLevel.None || enumValue == LogLevel.All)
                    {
                        enumValuesProcessed++;
                        continue;
                    }

                    var enumName = Enum.GetName(typeof(LogLevel), enumValue);

                    writer.WriteLine($"internal static void Log{enumName}(object data)");
                    writer.BeginBlock();
                    writer.WriteLine($"instance.Logger.Log{enumName}(data);");
                    writer.EndBlock();
                }
            }
        }

        private IEnumerator CreateContentProvider()
        {
            Writer writer = new Writer
            {
                buffer = new System.Text.StringBuilder()
            };
            var outputFileName = string.Empty;
            using (CodeGeneratorHelper helper = new CodeGeneratorHelper())
            {
                var className = modName + "ContentProvider";
                var fileName = className + ".cs";
                outputFileName = fileName;

                writer.WriteVerbatim(
@$"using RoR2.ContentManagement;
using UnityEngine;
using RoR2;
using System.Collections;");

                writer.WriteLine($"namespace {modName}");
                writer.BeginBlock(); //namespace begin

                writer.WriteLine($"public class {modName}Content : IContentPackProvider");
                writer.BeginBlock(); //class begin

                writer.WriteVerbatim(
    @$"public string identifier => {modName}Main.GUID;

// public static ReadOnlyContentPack readOnlyContentPack => new ReadOnlyContentPack({helper.MakeIdentifierPascalCase(modName)}ContentPack);
// internal static ContentPack {helper.MakeIdentifierPascalCase(modName)}ContentPack {{ get; }} = new ContentPack(); */");

                writer.WriteLine();

                writer.WriteLine("public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)"); //load static content method begin
                writer.BeginBlock();
                writer.WriteVerbatim(
@$"var asyncOperation = AssetBundle.LoadFromFileAsync({modName}Main.assetBundleDir);
while(!asyncOperation.isDone)
{{
    args.ReportProgress(asyncOperation.progress);
    yield return null;
}}");
                writer.EndBlock(); //Load static content method end

                writer.WriteLine($"public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)"); //Generate content pack method begin
                writer.BeginBlock();
                writer.WriteVerbatim(
$@"// ContentPack.Copy({helper.MakeIdentifierPascalCase(modName)}ContentPack, args.output);
args.ReportProgress(1f);
yield break;");
                writer.EndBlock(); //generate content pack method end;

                writer.WriteLine($"public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)"); //Finalize method begin
                writer.BeginBlock();
                writer.WriteVerbatim(
$@"args.ReportProgress(1f);
yield break;");
                writer.EndBlock(); //Finalize method end

                writer.WriteLine($"private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)"); //Add Self method begin
                writer.BeginBlock();
                writer.WriteLine($"addContentPackProvider(this);");
                writer.EndBlock(); //Add self method end

                writer.WriteLine($"internal {modName}Content()"); //Constructor begin
                writer.BeginBlock();
                writer.WriteLine($"ContentManager.collectContentPackProviders += AddSelf;");
                writer.EndBlock(); //Constructor End

                writer.EndBlock(); //class end
                writer.EndBlock(); //namespace end
            }

            var combinedPath = IOPath.Combine(_folderOutput, outputFileName);
            var validationData = new CodeGeneratorValidator.ValidationData
            {
                code = writer,
                desiredOutputPath = IOPath.GetFullPath(combinedPath)
            };
            var validationSubroutine = CodeGeneratorValidator.ValidateCoroutine(validationData);
            while (validationSubroutine.MoveNext())
            {
                yield return validationSubroutine.Current;
            }
            yield return 1f;
            yield break;
        }

        private IEnumerator CreateManifest()
        {
            yield break;
        }

        static ModWizard()
        {
            _defaultAssemblyDefinitions = new AssetGUID<AssemblyDefinitionAsset>[]
            {
                "a44f47cf3ada4435dbc516bad0bc86fe", //HLAPI
                "2bafac87e7f4b9b418d9448d219b01ab", //UGUI
                "6055be8ebefd69e48b49212b09b47b2f", //TMP
                "d60799ab2a985554ea1a39cd38695018", //PostProcessing
            };

            _defaultPrecompiledAssemblies = new string[]
            {
                "BepInEx.dll",
                "RoR2.dll",
                "Assembly-CSharp.dll",
                "RoR2BepInExPack.dll",
                "KinematicCharacterController.dll",
                "HGUnityUtils.dll",
                "HGCSharpUtils.dll",
                "LegacyResourcesAPI.dll",
                "Decalicious.dll",
                "Unity.Addressables.dll",
                "Mono.Cecil.dll",
                "UnityEngine.UI.dll",
                "Unity.TextMeshPro.dll",
                "MonoMod.RuntimeDetour.dll",
                "Unity.ResourceManager.dll",
                "MonoMod.Utils.dll",
                "Mono.Cecil.Pdb.dll",
            };
        }
    }
}
#endif