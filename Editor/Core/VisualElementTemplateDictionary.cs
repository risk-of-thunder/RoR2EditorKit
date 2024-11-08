using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="VisualElementTemplateDictionary"/> is a <see cref="ScriptableSingleton{T}"/> that contains a way to directly obtain a VisualTreeAsset via a key, usually the template's name.
    /// <br>This dictionary is serialized to improve performance.</br>
    /// </summary>
    [FilePath("ProjectSettings/RoR2EditorKit/VisualElementTemplateDictionary.asset", FilePathAttribute.Location.ProjectFolder)]
    public class VisualElementTemplateDictionary : ScriptableSingleton<VisualElementTemplateDictionary>
    {
        private static Dictionary<string, VisualTreeAsset> _templateTreeDictionary = new Dictionary<string, VisualTreeAsset>();
        private static TemplatePathValidator _defaultValidator = (path) => path.Contains(R2EKConstants.PACKAGE_NAME);
        [SerializeField] private List<TemplateNameToUXMLGuid> _serializedDictionary = new List<TemplateNameToUXMLGuid>();
        private static bool _didDomainReload = true;

        /// <summary>
        /// Saves the dictionary to disk
        /// </summary>
        public void DoSave()
        {
            Save(true);
        }

        /// <summary>
        /// Tries to obtain a template instance of the name <paramref name="templateName"/>
        /// </summary>
        /// <param name="templateName">The name of the template</param>
        /// <param name="target">The target visual element, if provided, the template's instance will be added to this</param>
        /// <param name="templatePathValidator">A validator to ensure that the template is correct.</param>
        /// <returns>A visual element with the template instance, if the template does not exists, it returns an error label and logs an error.</returns>
        public VisualElement GetTemplateInstance(string templateName, VisualElement target = null, TemplatePathValidator templatePathValidator = null)
        {
            var packageTemplate = LoadTemplate(templateName, templatePathValidator);
            var templatePath = AssetDatabase.GetAssetPath(packageTemplate);

            if (packageTemplate == null)
            {
                Debug.LogError($"Could not find Template {templateName}");
                Label errorLabel = new Label($"Could not find Template {templateName}");

                if (target != null)
                {
                    target.Add(errorLabel);
                    return errorLabel;
                }
                return errorLabel;
            }

            VisualElement instance = target;

            if (instance == null)
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

        /// <summary>
        /// Retrieves the VisualTreeAsset with the name <paramref name="templateName"/>
        /// </summary>
        /// <param name="templateName">The name of the template</param>
        /// <param name="validator">A validator to ensure that the template is correct.</param>
        /// <returns>The visual tree template</returns>
        public VisualTreeAsset LoadTemplate(string templateName, TemplatePathValidator validator = null)
        {
            if (_templateTreeDictionary.Count == 0)
            {
                ReloadDictionary();
            }

            if (_templateTreeDictionary.TryGetValue(templateName, out var asset))
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
            if (visualTreeAsset)
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
            bool hasChanges = false;
            for (int i = _serializedDictionary.Count - 1; i >= 0; i--)
            {
                TemplateNameToUXMLGuid template = _serializedDictionary[i];

                VisualTreeAsset asset = AssetDatabaseUtil.LoadAssetFromGUID<VisualTreeAsset>(template.v_guid);
                if (!asset)
                {
                    Debug.LogError("Cannot find VisualTreeAsset for template " + template.k_templateName);
                    _serializedDictionary.RemoveAt(i);
                    hasChanges = true;
                    continue;
                }

                if (_templateTreeDictionary.ContainsKey(template.k_templateName))
                {
                    Debug.LogWarning("Template for " + template.k_templateName + " is already in the dictionary, is this a duplicate entry?");
                    _serializedDictionary.RemoveAt(i);
                    hasChanges = true;
                    continue;
                }

                _templateTreeDictionary.Add(template.k_templateName, asset);
            }

            if (hasChanges)
                DoSave();
        }

        /// <summary>
        /// Represents a predicate that's used to validate a potential UXML asseet.
        /// </summary>
        public delegate bool TemplatePathValidator(string path);

        [Serializable]
        private struct TemplateNameToUXMLGuid
        {
            public string k_templateName;
            public GUID v_guid
            {
                get => new GUID(_hexRepresentation);
                set => _hexRepresentation = value.ToString();
            }
            [SerializeField]
            private string _hexRepresentation;
        }

        private class AssetPostprocessor : UnityEditor.AssetPostprocessor
        {
            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
            {
                if (_didDomainReload)
                {
                    _didDomainReload = false;
                    Debug.Log("Reloading Visual Element Template Finder.");
                    instance.ReloadDictionary();
                }
            }
        }
    }
}
