using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UIElements;

namespace RoR2.Editor.GameMaterialSystem
{
    [FilePath("ProjectSettings/RoR2EditorKit/GameMaterialSystemSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class GameMaterialSystemSettings : ScriptableSingleton<GameMaterialSystemSettings>
    {
        public ReadOnlyCollection<SerializableShaderWrapper> stubbedShaders => new ReadOnlyCollection<SerializableShaderWrapper>(_stubbedShaders);
        [SerializeField] private List<SerializableShaderWrapper> _stubbedShaders = new List<SerializableShaderWrapper>();
        private Dictionary<string, int> _addressableShaderNameToStubbedIndex = new Dictionary<string, int>();

        public void SaveSettings()
        {
            Save(true);
        }

        public bool AddShader(Shader shader, bool reloadDictionary = true)
        {
            var shaderName = shader.name;
            if(!shaderName.StartsWith("Stubbed"))
            {
                Debug.Log($"Shader {shader} does not start with the substring \"Stubbed\"! cannot add shader.");
                return false;
            }

            var substring = shaderName.Substring("Stubbed".Length);
            var address = $"{substring}.shader";
            Shader addressableShader = null;
            try
            {
                addressableShader = Addressables.LoadAssetAsync<Shader>(address).WaitForCompletion();
            }
            catch(Exception e)
            {
                Debug.LogError($"Stubbed shader {shader}'s real shader addres \"{address}\" threw an exception while loading. Either the address is not valid or you need to reimport the addressable catalog.\n{e}");
                return false;
            }

            for(int i = 0; i < _stubbedShaders.Count; i++)
            {
                if (_stubbedShaders[i].shader == shader)
                {
                    Debug.Log($"Stubbed shader {shader} is already within the collection.");
                    return false;
                }
            }

            _stubbedShaders.Add(new SerializableShaderWrapper(shader));

            if(reloadDictionary)
            {
                ReloadDictionary();
            }

            return true;
        }

        public SerializableShaderWrapper GetStubbedFromAddressableShaderName(string name)
        {
            if(_addressableShaderNameToStubbedIndex.Count == 0)
            {
                ReloadDictionary();
            }

            if(_addressableShaderNameToStubbedIndex.ContainsKey(name))
            {
                return _stubbedShaders[_addressableShaderNameToStubbedIndex[name]];
            }
            return null;
        }

        public Shader LoadAddressableShader(Shader stubbed)
        {
            var name = stubbed.name;
            var substring = name.Substring("Stubbed".Length);
            var address = $"{substring}.shader";
            try
            {
                var addressableShader = Addressables.LoadAssetAsync<Shader>(address).WaitForCompletion();
                return addressableShader;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load Addressable Shader of address {address}, Either the address does not point to an addressable asset or you may need to reimport the addressable catalog.");
                return null;
            }
        }

        public void ReloadDictionary()
        {
            _addressableShaderNameToStubbedIndex.Clear();
            for(int i = 0; i < _stubbedShaders.Count; i++)
            {
                var stubbedShaderWrapper = _stubbedShaders[i];
                var stubbedShader = stubbedShaderWrapper.shader;
                if (!stubbedShader)
                    continue;

                var shader = LoadAddressableShader(stubbedShader);
                if(shader)
                {
                    _addressableShaderNameToStubbedIndex.Add(shader.name, i);
                }
            }
        }

        [InitializeOnLoadMethod]
        private static void CreateDictionaryOnDomainReload()
        {
            var settings = instance;
            if(settings)
            {
                settings.ReloadDictionary();
            }
        }

        /// <summary>
        /// Opens the ProjectSettings window and selects these settings
        /// </summary>
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Game Material System")]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/RoR2EditorKit/Game Material System");
    }
}