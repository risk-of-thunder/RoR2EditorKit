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
        public SerializableShaderWrapper shader;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string json = File.ReadAllText(ctx.assetPath);
            var smd = JsonUtility.FromJson<SerializableMaterialData>(json);
            var material = smd.ToMaterial();

            shader = GameMaterialSystemSettings.instance.GetStubbedFromAddressableShaderName(material.shader.name);
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

        [MenuItem("Assets/BundleKit/Game Material")]
        static void CreateGameMaterial()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(path)) return;

            var material = new Material(Addressables.LoadAssetAsync<Shader>("RoR2/Base/Shaders/HGStandard.shader").WaitForCompletion());
            var smd = SerializableMaterialData.Build(material);

            string assetPath = Path.Combine(path, $"NewMaterial.{Extension}");
            File.WriteAllText(assetPath, JsonUtility.ToJson(smd));

            AssetDatabase.ImportAsset(assetPath);
        }
    }
}