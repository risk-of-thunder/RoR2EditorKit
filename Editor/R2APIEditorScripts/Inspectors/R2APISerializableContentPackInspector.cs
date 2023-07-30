#if RISKOFRAIN2 && RISKOFTHUNDER_R2API_CONTENTMANAGEMENT
using R2API.ScriptableObjects;
using RoR2EditorKit.Inspectors;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using RoR2EditorKit.VisualElements;
using ThunderKit.Core.Manifests;
using System.Reflection;
using EntityStates;
using System.Linq;
using ThunderKit.Core.Manifests.Datums;
using UnityEditorInternal;
using System.IO;
using ThunderKit.Core.Data;
using Object = UnityEngine.Object;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace RoR2EditorKit.R2APIRelated.Inspectors
{
    [CustomEditor(typeof(R2APISerializableContentPack))]
    public class R2APISerializableContentPackInspector : ScriptableObjectInspector<R2APISerializableContentPack>
    {
        private ValidatingObjectField manifestField;
        protected override bool HasVisualTreeAsset => true;
        protected override void DrawInspectorGUI()
        {
            manifestField = DrawInspectorElement.Q<ValidatingObjectField>("manifestField");
            SetupValidator(manifestField);
            DrawInspectorElement.Query<PropertyField>().ForEach(SetupContextMenu);
        }

        private void SetupValidator(ValidatingObjectField fld)
        {
            fld.objectField.SetObjectType<Manifest>();
            fld.objectField.SetValueWithoutNotify(Settings.mainManifest);
            fld.AddValidator(() =>
            {
                return !fld.objectField.value;
            },
            "No Manifest Selected, Please add your main manifest to the ROR2EK settings or select one manually.");
            fld.ForceValidation();
        }
        private void SetupContextMenu(PropertyField property)
        {
            property.AddSimpleContextMenu(new ContextMenuData
            {
                actionStatusCheck = HasManifestSelected,
                menuAction = AutoPopulate,
                menuName = "Auto Populate",
                userData = GetArrayType(property.bindingPath)
            });
        }

        private DropdownMenuAction.Status HasManifestSelected(DropdownMenuAction action)
        {
            return manifestField.objectField.value ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled;
        }

        private void AutoPopulate(DropdownMenuAction action)
        {
            if (!manifestField.objectField.value)
                return;

            var manifest = (Manifest)manifestField.objectField.value;
            var assemblyDefinitionDatum = manifest.Data.OfType<AssemblyDefinitions>().FirstOrDefault();
            var assetBundleDefinitionDatum = manifest.Data.OfType<AssetBundleDefinitions>().FirstOrDefault();
            ContentArray contentArray = (ContentArray)action.userData;

            if(contentArray.contentType == typeof(SerializableEntityStateType) && assemblyDefinitionDatum)
            {
                AddEntityStates(assemblyDefinitionDatum, contentArray);
            }
            else if(contentArray.contentType == typeof(GameObject) && assetBundleDefinitionDatum)
            {
                AddGameObjects(assetBundleDefinitionDatum, contentArray);
            }
            else if(assetBundleDefinitionDatum)
            {
                AddScriptableObjects(assetBundleDefinitionDatum, contentArray);
            }
        }

        private void AddEntityStates(AssemblyDefinitions assemblyDefinitionDatum, ContentArray contentArray)
        {
            Assembly[] assemblies = assemblyDefinitionDatum.definitions.Select(GetAssemblyFromAssemblyDefinition).ToArray();

            List<SerializableEntityStateType> serializableTypes = new List<SerializableEntityStateType>();
            foreach (var assembly in assemblies)
            {
                var entityStateTypes = assembly.GetTypesSafe()
                    .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EntityState)))
                    .Select(t => new SerializableEntityStateType(t));
                serializableTypes.AddRange(entityStateTypes);
            }

            contentArray.arrayField.SetValue(target, serializableTypes.ToArray());
        }

        private void AddGameObjects(AssetBundleDefinitions assetBundles, ContentArray contentArray)
        {
            List<Object> assetsFromBundleDefs = new List<Object>();
            foreach (var bundle in assetBundles.assetBundles)
            {
                GetAssetsFromBundle(bundle.assets, assetsFromBundleDefs, IsValid);
            }

            GameObject[] array = assetsFromBundleDefs.Cast<GameObject>().ToArray();
            contentArray.arrayField.SetValue(target, array);

            bool IsValid(Object obj)
            {
                if (obj is GameObject gameObject)
                {
                    switch(contentArray.fieldName)
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
            }
        }

        private void AddScriptableObjects(AssetBundleDefinitions assetBundles, ContentArray contentArray)
        {
            List<Object> assetsFromBundles = new List<Object>();
            foreach(var bundle in assetBundles.assetBundles)
            {
                GetAssetsFromBundle(bundle.assets, assetsFromBundles, IsValid);
            }

            var array = Array.CreateInstance(contentArray.contentType, assetsFromBundles.Count);
            for (int i = 0; i < array.Length; i++)
            {
                array.SetValue(assetsFromBundles[i], i);
            }
            contentArray.arrayField.SetValue(target, array);

            bool IsValid(Object obj)
            {
                return (obj is ScriptableObject && (obj.GetType() == contentArray.contentType || obj.GetType().IsSubclassOf(contentArray.contentType)));
            }
        }

        private static void GetAssetsFromBundle(IEnumerable<Object> inputAssets, List<Object> outputAssets, Func<Object, bool> filter)
        {
            foreach (var asset in inputAssets)
            {
                var assetPath = AssetDatabase.GetAssetPath(asset);

                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var files = Directory.GetFiles(assetPath, "*", SearchOption.AllDirectories);
                    var assets = files.Select(path => AssetDatabase.LoadAssetAtPath<Object>(path));
                    GetAssetsFromBundle(assets, outputAssets, filter);
                }
                else if (asset is UnityPackage up)
                {
                    GetAssetsFromBundle(up.AssetFiles, outputAssets, filter);
                }
                else if(filter(asset))
                {
                    outputAssets.Add(asset);
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

        private ContentArray GetArrayType(string bindingPath)
        {
            Type t = TargetType.GetType();
            FieldInfo fieldInfo = t.GetField(bindingPath, BindingFlags.Instance | BindingFlags.Public);
            Type collectionType = fieldInfo.FieldType;
            return new ContentArray
            {
                arrayField = fieldInfo,
                contentType = collectionType.GetElementType(),
                fieldName = bindingPath
            };
        }

        private class ContentArray
        {
            public FieldInfo arrayField;
            public Type contentType;
            public string fieldName;
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
#endif
