using System;
using UnityEditor;
using UnityEngine;
using static UnityEditor.ShaderUtil;

namespace RoR2.Editor.GameMaterialSystem
{
    [Serializable]
    public struct SerializableShaderProperty
    {
        public string name;
        public float floatValue;
        public Color colorValue;
        public Vector4 vectorValue;
        public TextureReference textureReference;
        public ShaderPropertyType type;
        public void SetValue<T>(T value)
        {
            switch (value)
            {
                case Color c when type == ShaderUtil.ShaderPropertyType.Color:
                    colorValue = c;
                    break;
                case Vector4 v when type == ShaderUtil.ShaderPropertyType.Vector:
                    vectorValue = v;
                    break;
                case float f when type == ShaderUtil.ShaderPropertyType.Range:
                    floatValue = f;
                    break;
                case float f when type == ShaderUtil.ShaderPropertyType.Float:
                    floatValue = f;
                    break;
                case TextureReference tr when type == ShaderUtil.ShaderPropertyType.TexEnv:
                    textureReference = tr;
                    break;
            }
        }
    }
}