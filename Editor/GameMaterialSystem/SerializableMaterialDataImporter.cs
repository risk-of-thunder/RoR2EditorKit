using System.IO;
using UnityEditor.AssetImporters;
using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace RoR2.Editor.GameMaterialSystem
{
    [ScriptedImporter(3, new[] { Extension })]
    public class SerializableMaterialDataImporter : ScriptedImporter
    {
        public const string Extension = "smd";

        [SerializeField]
        public SerializableShaderWrapper stubbedShader;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string json = File.ReadAllText(ctx.assetPath);
            var smd = JsonUtility.FromJson<SerializableMaterialData>(json);
            var material = smd.ToMaterial();
            if(material == null)
            {
                Debug.LogError($"Import for {this} failed. AddressableCatalog may be missing or corrupted.");
                return;
            }

            var paths = material.GetTexturePropertyNames()
                .Select(tpm => (Object)material.GetTexture(tpm))
                .Prepend(material.shader)
                .Select(asset =>
                {
                    if (asset && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out string guid, out long localId))
                        return AssetDatabase.GUIDToAssetPath(guid);

                    return string.Empty;
                })
                .Distinct()
                .Where(path => !string.IsNullOrEmpty(path))
                .ToArray();

            foreach (var texDependency in paths)
                ctx.DependsOnSourceAsset(texDependency);

            ctx.AddObjectToAsset(smd.identity, material);
        }

        [MenuItem("Assets/Create/Game Material")]
        static void CreateGameMaterial()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(path)) return;

            var stubbedStandard = GameMaterialSystemSettings.instance.stubbedHGStandard;
            if (!stubbedStandard)
                return;

            if(!GameMaterialSystemSettings.instance.TryLoadAddressableShader(stubbedStandard, out var addressableShader))
            {
                EditorUtility.DisplayDialog($"Failed to get Addressable Shader", $"The GameMaterialSystem was unable to retrieve the Addressable version of the shader \"{stubbedStandard}\". You may need to re-import the Addressable Catalog. No GameMaterial will be created.", "Understood");

                return;
            }

            var material = new Material(addressableShader);
            var smd = SerializableMaterialData.Build(material);

            string assetPath = Path.Combine(path, $"NewMaterial.{Extension}");
            File.WriteAllText(assetPath, JsonUtility.ToJson(smd));

            AssetDatabase.ImportAsset(assetPath);
        }
    }
}