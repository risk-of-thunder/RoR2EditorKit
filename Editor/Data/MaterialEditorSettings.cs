using RoR2EditorKit.Inspectors;
using System;
using System.Collections.Generic;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Data
{

    /// <summary>
    /// The RoR2EK Material Editor Settings
    /// </summary>
    public sealed class MaterialEditorSettings : ThunderKitSetting
    {
        /// <summary>
        /// Represents a pair of string and shader
        /// </summary>
        [Serializable]
        public class ShaderStringPair
        {
            /// <summary>
            /// The shader's name, ideally this should be the File name, not the actual shader.name
            /// </summary>
            public string shaderName;
            /// <summary>
            /// The shader that belongs to this pair
            /// </summary>
            public SerializableShaderWrapper shader;
        }

        private SerializedObject materialEditorSettingsSO;

        /// <summary>
        /// Wether the material editor system is enabled or disabled
        /// </summary>
        public bool enableMaterialEditor = true;

        [SerializeField] private string myString = "Lol";
        /// <summary>
        /// The Shader String Pairs of the Material Editor Setting
        /// </summary>
        public List<ShaderStringPair> shaderStringPairs = new List<ShaderStringPair>();

        /// <summary>
        /// Direct access to the main settings file
        /// </summary>
        public RoR2EditorKitSettings MainSettings { get => GetOrCreateSettings<RoR2EditorKitSettings>(); }

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            if (materialEditorSettingsSO == null)
                materialEditorSettingsSO = new SerializedObject(this);

            rootElement.Add(MaterialEditorSettingsInspector.StaticInspectorGUI(materialEditorSettingsSO, true));

            rootElement.Bind(materialEditorSettingsSO);
        }

        /// <summary>
        /// Creates a new ShaderStringPair if no shader string pair exists.
        /// </summary>
        /// <param name="shaderName">The asset name of the shader</param>
        /// <param name="callingType">The type that's calling this method.</param>
        public void CreateShaderStringPairIfNull(string shaderName, Type callingType)
        {
            var pair = shaderStringPairs.Find(x => x.shaderName == shaderName);
            if (pair == null)
            {
                ShaderStringPair shaderStringPair = new ShaderStringPair
                {
                    shader = null,
                    shaderName = shaderName
                };

                shaderStringPairs.Add(shaderStringPair);
            }
        }
    }
}