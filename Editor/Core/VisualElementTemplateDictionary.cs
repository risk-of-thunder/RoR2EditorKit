using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    [FilePath("ProjectSettings/RoR2EditorKit/VisualElementTemplateDictionary.asset", FilePathAttribute.Location.ProjectFolder)]
    public class VisualElementTemplateDictionary : ScriptableSingleton<VisualElementTemplateDictionary>
    {
        private static Dictionary<string, VisualTreeAsset> _templateTreeDictionary = new Dictionary<string, VisualTreeAsset>();
        private static TemplatePathValidator _defaultValidator = (path) => path.Contains(R2EKConstants.PACKAGE_NAME);
        [SerializeField] private List<TemplateNameToUXMLGuid> _serializedDictionary = new List<TemplateNameToUXMLGuid>();
        private static bool _didDomainReload = true;
        public void DoSave()
        {
            Save(true);
        }

        public VisualElement GetTemplateInstance(string templateName, VisualElement target = null, TemplatePathValidator templatePathValidator = null)
        {
            var packageTemplate = LoadTemplate(templateName, templatePathValidator);
            var templatePath = AssetDatabase.GetAssetPath(packageTemplate);

            if(packageTemplate == null)
            {
                Debug.LogError($"Could not find Template {templateName}");
                Label errorLabel = new Label($"Could not find Template {templateName}");

                if(target != null)
                {
                    target.Add(errorLabel);
                    return errorLabel;
                }
                return errorLabel;
            }

            VisualElement instance = target;

            if(instance == null)
            {
                instance = packageTemplate.CloneTree();
            }
            else
            {
                packageTemplate.CloneTree(instance);
            }

            instance.AddToClassList("grow");

            return instance;
        }

        public VisualTreeAsset LoadTemplate(string templateName, TemplatePathValidator validator = null)
        {
            if(_templateTreeDictionary.Count == 0)
            {
                ReloadDictionary();
            }

            if(_templateTreeDictionary.TryGetValue(templateName, out var asset))
            {
                return asset;
            }

            return CreateTemplate(templateName, validator ?? _defaultValidator);
        }

        private VisualTreeAsset CreateTemplate(string templateName, TemplatePathValidator validator)
        {
            var searchResults = AssetDatabaseUtil.FindAssetPaths(templateName, R2EKConstants.FolderPaths.findAllFolders);
            searchResults.Select(path => path.Replace("\\", "/"));
            var templatePath = searchResults
                .Where(path => System.IO.Path.GetFileNameWithoutExtension(path).Equals(templateName, StringComparison.CurrentCultureIgnoreCase))
                .Where(path => System.IO.Path.GetExtension(path).Equals(".uxml", StringComparison.CurrentCultureIgnoreCase))
                .Where(path => validator(path))
                .FirstOrDefault();

            var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(templatePath);
            if(visualTreeAsset)
            {
                _serializedDictionary.Add(new TemplateNameToUXMLGuid { k_templateName = templateName, v_guid = AssetDatabaseUtil.GetAssetGUID(visualTreeAsset) });
                _templateTreeDictionary.Add(templateName, visualTreeAsset);
                DoSave();
            }
            return visualTreeAsset;
        }

        private void ReloadDictionary()
        {
            _templateTreeDictionary = new Dictionary<string, VisualTreeAsset>();
            for(int i = _serializedDictionary.Count - 1; i >= 0; i--)
            {
                TemplateNameToUXMLGuid template = _serializedDictionary[i];

                VisualTreeAsset asset = AssetDatabaseUtil.LoadAssetFromGUID<VisualTreeAsset>(template.v_guid);
                if(!asset)
                {
                    Debug.LogError("Cannot find VisualTreeAsset for template " + template.k_templateName);
                    _serializedDictionary.RemoveAt(i);
                    continue;
                }

                if (_templateTreeDictionary.ContainsKey(template.k_templateName))
                {
                    Debug.LogWarning("Template for " + template.k_templateName + " is already in the dictionary, is this a duplicate entry?");
                    _serializedDictionary.RemoveAt(i);
                    continue;
                }

                _templateTreeDictionary.Add(template.k_templateName, asset);
            }
        }

        public delegate bool TemplatePathValidator(string path);
        
        [Serializable]
        private struct TemplateNameToUXMLGuid
        {
            public string k_templateName;
            public GUID v_guid;
        }

        private class AssetPostprocessor : UnityEditor.AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if(_didDomainReload)
                {
                    _didDomainReload = false;
                    Debug.Log("Reloading Visual Element Template Finder.");
                    instance.ReloadDictionary();
                }
            }
        }
    }
}
