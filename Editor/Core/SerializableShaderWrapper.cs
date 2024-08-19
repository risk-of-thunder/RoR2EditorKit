using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoR2.Editor
{
    [Serializable]
    public class SerializableShaderWrapper
    {
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
                _shaderGuid = !value ? string.Empty : AssetDatabaseUtil.GetAssetGUID(value);
            }
        }

        [SerializeField] private string _shaderName;
        [SerializeField] private string _shaderGuid;
        public SerializableShaderWrapper(Shader shaderToSerialize)
        {
            shader = shaderToSerialize;
        }
    }
}