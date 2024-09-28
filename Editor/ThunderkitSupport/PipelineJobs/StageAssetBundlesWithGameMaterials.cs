using ThunderKit.Core.Attributes;
using System.Threading.Tasks;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using System;
using IOPath = System.IO.Path;
using Object = UnityEngine.Object;
using UnityEditor.Build.Content;
using UnityEngine;
using System.Linq;
using ThunderKit.Core.Paths;
using System.IO;
using NUnit.Framework;
using System.Collections.Generic;
using ThunderKit.Core.Data;
using UnityEditor.Build.Pipeline;
using BuildCompression = UnityEngine.BuildCompression;
using System.Text;
using UnityEditor.Compilation;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Build.Pipeline.Tasks;
#if R2EK_GAME_MATERIAL_SYSTEM
using RoR2.Editor.GameMaterialSystem;
#endif

namespace RoR2.Editor.PipelineJobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(AssetBundleDefinitions))]
    public class StageAssetBundlesWithGameMaterials : PipelineJob
    {
#if !R2EK_GAME_MATERIAL_SYSTEM || !R2EK_SCRIPTABLEBUILDPIPELINE
        [Header("Cannot use this pipeline because the Game Material System is not Enabled or ScriptableBuildPipeline 1.2.0 or greater is not installed.")]
        public int num;
#else
        [EnumFlag]
        public ContentBuildFlags contentBuildFlags = ContentBuildFlags.None;
        public BuildTarget buildTarget = BuildTarget.StandaloneWindows;
        public BuildTargetGroup buildTargetGroup = BuildTargetGroup.Standalone;
        public Compression compression = Compression.Uncompressed;
        public bool simulate;

        [PathReferenceResolver]
        public string bundleArtifactPath = "<AssetBundleStaging>";

        public override Task Execute(Pipeline pipeline)
        {
            var assetBundleDefs = pipeline.Datums.OfType<AssetBundleDefinitions>().ToArray();
            if (assetBundleDefs.Length == 0)
                return Task.CompletedTask;

            var bundleArtifactPath = this.bundleArtifactPath.Resolve(pipeline, this);
            Directory.CreateDirectory(bundleArtifactPath);

            var explicitAssets = assetBundleDefs.SelectMany(abd => abd.assetBundles)
                .SelectMany(ab => ab.assets)
                .Where(a => a)
                .ToArray();

            var explicitAssetPaths = new List<string>();
            PopulateWithExplicitAssets(explicitAssets, explicitAssetPaths);

            foreach(var path in explicitAssetPaths)
            {
                pipeline.Log(ThunderKit.Core.Pipelines.LogLevel.Information, path);
            }

            var builds = GetAssetBundleBuilds(assetBundleDefs, explicitAssetPaths);

            if (simulate)
                return Task.CompletedTask;

            var buildParams = CreateParams(bundleArtifactPath);

            var content = new BundleBuildContent(builds);

            var context = new List<IContextObject>();
            var returnCode = ContentPipeline.BuildAssetBundles(buildParams, content, out var result, BuildTaskList(), context.ToArray());

            if (returnCode < 0)
                throw new Exception($"AssetBundle build incomplete: {returnCode}");

            CopyModifiedAssetBundles(bundleArtifactPath, pipeline);
            
            return Task.CompletedTask;
        }

        private void CopyModifiedAssetBundles(string bundleArtifactPath, Pipeline pipeline)
        {
            for (pipeline.ManifestIndex = 0; pipeline.ManifestIndex < pipeline.Manifests.Length; pipeline.ManifestIndex++)
            {
                var manifest = pipeline.Manifest;
                foreach (var assetBundleDef in manifest.Data.OfType<AssetBundleDefinitions>())
                {
                    var bundleNames = assetBundleDef.assetBundles.Select(ab => ab.assetBundleName).ToArray();
                    foreach (var outputPath in assetBundleDef.StagingPaths.Select(path => path.Resolve(pipeline, this)))
                    {
                        foreach (string dirPath in Directory.GetDirectories(bundleArtifactPath, "*", SearchOption.AllDirectories))
                            Directory.CreateDirectory(dirPath.Replace(bundleArtifactPath, outputPath));

                        foreach (string filePath in Directory.GetFiles(bundleArtifactPath, "*", SearchOption.AllDirectories))
                        {
                            bool found = false;
                            foreach (var bundleName in bundleNames)
                            {
                                if (filePath.IndexOf(bundleName, StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) continue;
                            var destFolder = IOPath.GetDirectoryName(filePath.Replace(bundleArtifactPath, outputPath));
                            var destFileName = IOPath.GetFileName(filePath);
                            Directory.CreateDirectory(destFolder);
                            FileUtil.ReplaceFile(filePath, IOPath.Combine(destFolder, destFileName));
                        }

                        //.manifest doesn't exist when building with ContentPipeline.BuildAssetBundles()
                        //but do we really need it?
                        //
                        //var manifestSource = Path.Combine(bundleArtifactPath, $"{Path.GetFileName(bundleArtifactPath)}.manifest");
                        //var manifestDestination = Path.Combine(outputPath, $"{manifest.Identity.Name}.manifest");
                        //FileUtil.ReplaceFile(manifestSource, manifestDestination);
                    }
                }
            }
            pipeline.ManifestIndex = -1;
        }

        private IList<IBuildTask> BuildTaskList()
        {
            var tasks = new List<IBuildTask>
            {
                new SwitchToBuildPlatform(),
                new RebuildSpriteAtlasCache(),

                new PostScriptsCallback(),

                new CalculateAssetDependencyData(),
                new CalculateSceneDependencyData(),
                new StripUnusedSpriteSources(),
                new PostDependencyCallback(),

                new GenerateBundlePacking(),
                new UpdateBundleObjectLayout(),
                new GenerateBundleCommands(),
                new GenerateSubAssetPathMaps(),
                new GenerateBundleMaps(),
                new PostPackingCallback(),

                new WriteSerializedFiles(),
                new ArchiveAndCompressBundles(),
                new PostWritingCallback()
            };

            return tasks;
        }

        private BundleBuildParameters CreateParams(string outputPath)
        {
            var compressionStruct = default(BuildCompression);
            switch (compression)
            {
                case Compression.Uncompressed: compressionStruct = BuildCompression.Uncompressed; break;
                case Compression.LZMA: compressionStruct = BuildCompression.LZMA; break;
                case Compression.LZ4: compressionStruct = BuildCompression.LZ4; break;
            }

            return new BundleBuildParameters(buildTarget, buildTargetGroup, outputPath)
            {
                BundleCompression = compressionStruct,
                ContentBuildFlags = contentBuildFlags
            };
        }

        private AssetBundleBuild[] GetAssetBundleBuilds(AssetBundleDefinitions[] assetBundleDefinitions, List<string> explicitAssetPaths)
        {
            var ignoredExtensions = new[] { ".dll", ".cs", ".meta" };
            var logBuilder = new StringBuilder();
            var stubbedShaders = GameMaterialSystemSettings.instance.stubbedShaders.Select(ssw => ssw.shader).ToArray();
            int definedBundleCount = assetBundleDefinitions.Sum(abd => abd.assetBundles.Length);
            var builds = new AssetBundleBuild[definedBundleCount];
            logBuilder.AppendLine($"Defining {builds.Length} AssetBundleBuilds");

            var buildsIndex = 0;
            for(int defIndex = 0; defIndex < assetBundleDefinitions.Length; defIndex++)
            {
                var assetBundleDef = assetBundleDefinitions[defIndex];
                var playerAssemblies = CompilationPipeline.GetAssemblies();
                var assemblyFiles = playerAssemblies.Select(pa => pa.outputPath).ToList();
                var sourceFiles = playerAssemblies.Select(pa => pa.sourceFiles).ToArray();

                for(int i = 0; i < assetBundleDef.assetBundles.Length; i++)
                {
                    var def = assetBundleDef.assetBundles[i];
                    ref var build = ref builds[buildsIndex];
                    var assets = new List<string>();

                    logBuilder.AppendLine("--------------------------------");
                    logBuilder.AppendLine($"Defining Bundle: {def.assetBundleName}");
                    logBuilder.AppendLine();

                    var firstAsset = def.assets.FirstOrDefault(x => x is SceneAsset);

                    if (firstAsset != null)
                    {
                        assets.Add(AssetDatabase.GetAssetPath(firstAsset));
                    }
                    else
                    {
                        PopulateWithExplicitAssets(def.assets, assets);

                        var dependencies = assets.SelectMany(assetPath => AssetDatabase.GetDependencies(assetPath))
                            .Where(assetPath => !ignoredExtensions.Contains(IOPath.GetExtension(assetPath)))
                            .Where(dap => !explicitAssetPaths.Contains(dap));

                        assets.AddRange(dependencies);
                    }

                    build.assetNames = assets
                        .Select(ap => ap.Replace("\\", "/"))
                        .Where(dap => !AssetDatabase.IsValidFolder(dap))
                        .Distinct()
                        .ToArray();

                    build.assetBundleName = def.assetBundleName;
                    buildsIndex++;

                    foreach(var asset in build.assetNames)
                    {
                        logBuilder.AppendLine(asset);
                    }
                    logBuilder.AppendLine("--------------------------------");
                    logBuilder.AppendLine();
                }
            }

            Debug.Log(logBuilder.ToString());

            return builds;
        }

        private void PopulateWithExplicitAssets(IEnumerable<Object> inputAssets, List<string> outputAssets)
        {
            foreach (var asset in inputAssets)
            {
                if (!asset)
                {
                    continue;
                }
                var assetPath = AssetDatabase.GetAssetPath(asset);

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var files = Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);
                    var assets = files.Select(path => AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
                    PopulateWithExplicitAssets(assets, outputAssets);
                }
                else if (asset is UnityPackage up)
                {
                    PopulateWithExplicitAssets(up.AssetFiles, outputAssets);
                }
                else
                {
                    outputAssets.Add(assetPath);
                }
            }
        }

        public enum Compression
        {
            Uncompressed,
            LZMA,
            LZ4
        }
#endif
    }
}
