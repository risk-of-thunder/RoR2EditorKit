using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class Inspector<T> : UnityEditor.Editor where T : UnityEngine.Object
    {
        public EditorSetting inspectorProjectSettings
        {
            get
            {
                if (_inspectorProjectSettings == null)
                {
                    _inspectorProjectSettings = EditorSettingManager.GetOrCreateSettingsFor(typeof(T), EditorSettingManager.SettingType.ProjectSetting);
                }
                return _inspectorProjectSettings;
            }
        }
        private EditorSetting _inspectorProjectSettings;

        public EditorSetting inspectorPreferenceSettings
        {
            get
            {
                if (_inspectorPreferenceSettings == null)
                {
                    _inspectorPreferenceSettings = EditorSettingManager.GetOrCreateSettingsFor(typeof(T), EditorSettingManager.SettingType.UserSetting);
                }
                return _inspectorPreferenceSettings;
            }
        }
        private EditorSetting _inspectorPreferenceSettings;

        public bool inspectorEnabled
        {
            get
            {
                return inspectorPreferenceSettings.GetOrCreateSetting("isInspectorEnabled", false);
            }
            set
            {
                var origValue = inspectorPreferenceSettings.GetOrCreateSetting("isInspectorEnabled", false);
                if (origValue != value)
                {
                    inspectorPreferenceSettings.SetSettingValue("isInspectorEnabled", value);
                    OnInspectorEnabledChange();
                }
            }
        }

        public VisualElement rootVisualElement
        {
            get
            {
                if(_rootVisualElement == null)
                {
                    _rootVisualElement = new VisualElement();
                    _rootVisualElement.name = $"{typeof(T).Name}_RootElement";
                }
                return _rootVisualElement;
            }
        }
        private VisualElement _rootVisualElement;

        private void OnInspectorEnabledChange()
        {
            
        }

        public sealed override VisualElement CreateInspectorGUI()
        {
            CreateInspectorUI();
            return rootVisualElement;
        }

        protected abstract void CreateInspectorUI();
    }
}