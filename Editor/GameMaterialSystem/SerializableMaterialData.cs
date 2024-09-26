using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static UnityEditor.ShaderUtil;

namespace RoR2.Editor.GameMaterialSystem
{
    [System.Serializable]
    public struct SerializableMaterialData
    {
        public string identity;
        public string name;
        public SerializableShaderWrapper shaderWrapper;
        public bool doubleSidedGI;
        public bool enableInstancing;
        public int renderQueue;
        public MaterialGlobalIlluminationFlags globalIlluminationFlags;
        public string[] shaderKeywords;
        public SerializableShaderProperty[] properties;

        public static SerializableMaterialData Build(Material material, string identity = null)
        {
            var serializedMaterial = new SerializableMaterialData();
            var addressableShader = material.shader;

            var stubbedWrapper = GameMaterialSystemSettings.instance.GetStubbedFromAddressableShaderName(addressableShader.name);

            serializedMaterial.name = material.name;
            serializedMaterial.shaderWrapper = stubbedWrapper;

            var stubbedShader = stubbedWrapper.shader;
            var dataList = new List<SerializableShaderProperty>();
            for (int i = 0; i < GetPropertyCount(stubbedShader); i++)
            {
                var data = new SerializableShaderProperty
                {
                    name = GetPropertyName(stubbedShader, i),
                    type = GetPropertyType(stubbedShader, i)
                };
                switch (data.type)
                {
                    case ShaderPropertyType.Color:
                        data.SetValue(material.GetColor(data.name));
                        break;
                    case ShaderPropertyType.Float:
                        data.SetValue(material.GetFloat(data.name));
                        break;
                    case ShaderPropertyType.Vector:
                        data.SetValue(material.GetVector(data.name));
                        break;
                    case ShaderPropertyType.Range:
                        data.SetValue(material.GetFloat(data.name));
                        break;
                    case ShaderPropertyType.TexEnv:
                        var offset = material.GetTextureOffset(data.name);
                        var scale = material.GetTextureScale(data.name);
                        var textEnv = material.GetTexture(data.name);
                        if (textEnv && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(textEnv, out string guid, out long localId))
                        {
                            data.SetValue(new TextureReference
                            {
                                guid = guid,
                                offset = offset,
                                scale = scale
                            });
                        }
                        break;
                    default:
                        throw new InvalidOperationException($"Property: {data.name} has unsupported type: {data.type}");
                }
                dataList.Add(data);
            }

            serializedMaterial.properties = dataList.ToArray();
            serializedMaterial.doubleSidedGI = material.doubleSidedGI;
            serializedMaterial.enableInstancing = material.enableInstancing;
            serializedMaterial.renderQueue = material.renderQueue;
            serializedMaterial.shaderKeywords = material.shaderKeywords;
            serializedMaterial.globalIlluminationFlags = material.globalIlluminationFlags;
            serializedMaterial.identity = identity ?? GUID.Generate().ToString();
            return serializedMaterial;
        }

        public Material ToMaterial()
        {
            Shader shaderObj = shaderWrapper.shader;
            Shader addressableShader = GameMaterialSystemSettings.instance.LoadAddressableShader(shaderObj);
            Material material = new Material(addressableShader);
            material.doubleSidedGI = doubleSidedGI;
            material.enableInstancing = enableInstancing;
            material.renderQueue = renderQueue;
            material.shaderKeywords = shaderKeywords;
            material.globalIlluminationFlags = globalIlluminationFlags;
            foreach (var data in properties)
            {
                switch (data.type)
                {
                    case ShaderPropertyType.Color:
                        material.SetColor(data.name, data.colorValue);
                        break;
                    case ShaderPropertyType.Float:
                        material.SetFloat(data.name, data.floatValue);
                        break;
                    case ShaderPropertyType.Vector:
                        material.SetVector(data.name, data.vectorValue);
                        break;
                    case ShaderPropertyType.Range:
                        material.SetFloat(data.name, data.floatValue);
                        break;
                    case ShaderPropertyType.TexEnv:
                        try
                        {
                            var textureReference = data.textureReference;
                            var path = AssetDatabase.GUIDToAssetPath(textureReference.guid);
                            var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                            Texture texture = null;
                            switch (mainAsset)
                            {
                                case Texture tex:
                                    texture = tex;
                                    break;
                            }
                            if (texture)
                            {
                                material.SetTexture(data.name, texture);
                                material.SetTextureOffset(data.name, textureReference.offset);
                                material.SetTextureScale(data.name, textureReference.scale);
                            }
                        }
                        catch { }
                        break;
                    default:
                        throw new InvalidOperationException($"Property: {data.name} has unsupported type: {data.type}");
                }
            }
            return material;
        }
    }
}