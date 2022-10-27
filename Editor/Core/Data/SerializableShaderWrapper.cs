using RoR2EditorKit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.Core
{
    /// <summary>
    /// Serializing shader objects directly seems to cause issues, as such, it is recommended to use this shader wrapper.
    /// <para>instead of serializing the shader object itself, what gets serialized is the GUID and the shader name</para>
    /// </summary>
    [Serializable]
    public class SerializableShaderWrapper
    {
        public SerializableShaderWrapper(Shader shaderToSerialize)
        {
            SetShader(shaderToSerialize);
        }

        [SerializeField] private string shaderName;
        [SerializeField] private string shaderGUID;

        /// <summary>
        /// Loads the shader that was serialized from this SerializableShaderWrapper
        /// </summary>
        /// <returns>The serialized shader</returns>
        public Shader LoadShader()
        {
            Shader shader = Shader.Find(shaderName);
            if (!shader)
                shader = AssetDatabaseUtils.LoadAssetFromGUID<Shader>(shaderGUID);

            return shader;
        }

        /// <summary>
        /// Sets the shader that this SerializableShaderWrapper will serialize
        /// </summary>
        /// <param name="shader">The shader to serialize</param>
        public void SetShader(Shader shader)
        {
            shaderName = !shader ? string.Empty : shader.name;
            shaderGUID = !shader ? string.Empty : AssetDatabaseUtils.GetGUIDFromAsset(shader);
        }
    }
}
