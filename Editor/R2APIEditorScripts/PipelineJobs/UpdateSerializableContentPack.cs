/*#if RISKOFRAIN2 && RISKOFTHUNDER_R2API_CONTENTMANAGEMENT
using R2API.ScriptableObjects;
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Manifests.Datums;
using ThunderKit.Core.Pipelines;
using UnityEditorInternal;
using UnityEngine;
using static RoR2EditorKit.R2APIRelated.SerializableContentPackUtil;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace RoR2EditorKit.R2APIRelated.PipelineJobs
{
    [PipelineSupport(typeof(Pipeline)), RequiresManifestDatumType(typeof(AssetBundleDefinitions))]
    public sealed class UpdateSerializableContentPack : PipelineJob
    {
        public UpdateCollections collectionsToUpdate;
        
        public override Task Execute(Pipeline pipeline)
        {
            var manifest = pipeline.manifest;
            var datum = manifest.Data.OfType<AssetBundleDefinitions>().FirstOrDefault();
            if(!datum)
            {
                pipeline.Log(LogLevel.Error, "Pipeline \"UpdateSerializableContentPack\" requires an AssetBundleDefinitionsDatum to work, but no datum was found.");
                return Task.CompletedTask;
            }

            R2APISerializableContentPack contentPack = null;
            foreach(var bundle in datum.assetBundles)
            {
                List<Object> list = new List<Object>();
                AssetDatabaseUtils.GetAllAssetsFromInput(bundle.assets, list, asset =>
                {
                    return asset is R2APISerializableContentPack;
                });
                var pack = list.FirstOrDefault();
                if(pack)
                {
                    contentPack = (R2APISerializableContentPack)pack;
                    break;
                }
            }
            
            if(!contentPack)
            {
                pipeline.Log(LogLevel.Warning, "Could not find a SerializableContentPack in any of the assetbundles.");
                return Task.CompletedTask;
            }

            foreach(UpdateCollections flagValue in Enum.GetValues(typeof(UpdateCollections)))
            {
                if(!collectionsToUpdate.HasFlag(flagValue))
                {
                    continue;
                }

                var collection = new SerializableContentPackUtil.ContentArray(contentPack, Enum.GetName(typeof(UpdateCollections), flagValue));
                if(flagValue == UpdateCollections.entityStateTypes)
                {
                    var assemblyDatums = manifest.Data.OfType<AssemblyDefinitions>().FirstOrDefault();
                    if(!assemblyDatums)
                    {
                        pipeline.Log(LogLevel.Error, "colleectionsToUpdate has flag \"entityStateTypes\", but the manifest doesnt define an AssemblyDefinitions datum");
                        continue;
                    }
                    Assembly[] assemblies = assemblyDatums.definitions.Select(GetAssemblyFromAssemblyDefinition).ToArray();
                    SerializableContentPackUtil.SetEntityStates(assemblies, collection);
                }
                else if(flagValue.HasFlag(UpdateCollections.bodyPrefabs | UpdateCollections.masterPrefabs | UpdateCollections.projectilePrefabs | UpdateCollections.gameModePrefabs | UpdateCollections.effectPrefabs | UpdateCollections.networkedObjectPrefabs))
                {
                    List<Object> assetsFromBundleDefs = new List<Object>();
                    foreach (var bundle in datum.assetBundles)
                    {
                        AssetDatabaseUtils.GetAllAssetsFromInput(bundle.assets, assetsFromBundleDefs, o =>
                        {
                            if (o is GameObject gameObject)
                            {
                                switch (contentArray.fieldName)
                                {
                                    case "effectPrefabs": return gameObject.GetComponent<EffectComponent>();
                                    case "networkedObjectPrefabs":
                                        return gameObject.GetComponent<NetworkIdentity>() && !(gameObject.GetComponent<CharacterBody>() || gameObject.GetComponent<CharacterMaster>() || gameObject.GetComponent<ProjectileController>() || gameObject.GetComponent<Run>());
                                    case "bodyPrefabs": return gameObject.GetComponent<CharacterBody>();
                                    case "masterPrefabs": return gameObject.GetComponent<CharacterMaster>();
                                    case "projectilePrefabs": return gameObject.GetComponent<ProjectileController>();
                                    case "gameModePrefabs": return gameObject.GetComponent<Run>();
                                }
                            }
                            return false;
                        });
                    }

                    SerializableContentPackUtil.SetGameObjects(assetsFromBundleDefs.OfType<GameObject>(), contentArray);

                    bool IsValid(Object obj)
                    {
                        
                    }
                }
            }
        }

        private Assembly GetAssemblyFromAssemblyDefinition(AssemblyDefinitionAsset a)
        {
            AsmDef asmDef = JsonUtility.FromJson<AsmDef>(a.text);
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(ass =>
            {
                return ass.GetName().Name == asmDef.name;
            });
        }

        [Flags]
        public enum UpdateCollections : uint
        {
            None = 0,
            bodyPrefabs = 1,
            masterPrefabs = 2,
            projectilePrefabs = 4,
            gameModePrefabs = 8,
            effectPrefabs = 16,
            networkedObjectPrefabs = 32,
            skillDefs = 64,
            skillFamilies = 128,
            sceneDefs = 256,
            itemDefs = 512,
            itemTierDefs = 1024,
            itemRelationshipProviders = 2048,
            itemRelationshipTypes = 4096,
            equipmentDefs = 8192,
            buffDefs = 16384,
            eliteDefs = 32768,
            unlockableDefs = 65536,
            survivorDefs = 131072,
            artifactDefs = 262144,
            surfaceDefs = 524288,
            networkSoundEventDefs = 1048576,
            musicTrackDefs = 2097152,
            gameEndingDefs = 4194304,
            miscPickupDefs = 8388608,
            entityStateConfigurations = 16777216,
            entityStateTypes = 33554432,
            expansionDefs = 67108864,
            entitlementDefs = 134217728,
            Everything = ~None
        }


        [Serializable]
        struct AsmDef
        {
            public string name;
            public bool allowUnsafeCode;
            public bool overrideReferences;
            public bool autoReferenced;
            public string[] optionalUnityReferences;
            public string[] includePlatforms;
            public string[] excludePlatforms;
            public string[] precompiledReferences;
            public string[] defineConstraints;
        }
    }
}
#endif*/