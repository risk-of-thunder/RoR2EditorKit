using System;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// Serializing shader objects directly seems to cause issues, as such, it is recommended to use this shader wrapper.
    /// <para>instead of serializing the shader object itself, what gets serialized is the GUID and the shader name</para>
    /// </summary>
    [Serializable]
    public class SerializableShaderWrapper
    {
        /// <summary>
        /// Gets or sets the shader wrapped by this wrapper.
        /// </summary>
        public Shader shader
        {
            get
            {
                Shader shader = Shader.Find(_shaderName);
                if (!shader)
                    shader = AssetDatabaseUtil.LoadAssetFromGUID<Shader>(_shaderGuid);

                return shader;
            }
            set
            {
                _shaderName = !value ? string.Empty : value.name;
                _shaderGuid = !value ? string.Empty : AssetDatabaseUtil.GetAssetGUIDString(value);
            }
        }

        [SerializeField] internal string _shaderName;
        [SerializeField] internal string _shaderGuid;

        /// <summary>
        /// Creates a new instance of a SerializableShaderWrapper
        /// </summary>
        public SerializableShaderWrapper(Shader shaderToSerialize)
        {
            shader = shaderToSerialize;
        }
    }
}