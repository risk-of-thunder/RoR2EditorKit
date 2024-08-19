using EntityStates;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
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
                    _inspectorProjectSettings = EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.ProjectSetting);
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
                    _inspectorPreferenceSettings = EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.UserSetting);
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
                if (_rootVisualElement == null)
                {
                    _rootVisualElement = new VisualElement();
                    _rootVisualElement.name = $"{typeof(T).Name}_RootElement";
                }
                return _rootVisualElement;
            }
        }
        private VisualElement _rootVisualElement;

        protected bool hasDoneFirstDrawing { get; private set; }
        protected event Action onRootElementCleared;

        private void OnInspectorEnabledChange()
        {
            rootVisualElement.Clear();
            onRootElementCleared?.Invoke();

            if (!inspectorEnabled)
            {
                var imguiContainer = new IMGUIContainer(DoDrawDefaultInspector);
                imguiContainer.name = $"{typeof(T).Name}_DefaultInspector";
                rootVisualElement.Add(imguiContainer);
            }
            else
            {
                var inspectorElement = CreateInspectorUI();
                rootVisualElement.Add(inspectorElement);
                if (hasDoneFirstDrawing)
                    rootVisualElement.Bind(serializedObject);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DoDrawDefaultInspector() => DrawDefaultInspector();

        public sealed override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            OnInspectorEnabledChange();
            serializedObject.ApplyModifiedProperties();
            hasDoneFirstDrawing = true;
            return rootVisualElement;
        }

        protected abstract VisualElement CreateInspectorUI();

        protected T1 GetOrCreateSetting<T1>(string settingName, T1 defaultValue, EditorSettingManager.SettingType settingType)
        {
            EditorSetting src = null;
            switch(settingType)
            {
                case EditorSettingManager.SettingType.ProjectSetting:
                    src = inspectorProjectSettings;
                    break;
                case EditorSettingManager.SettingType.UserSetting:
                    src = inspectorPreferenceSettings;
                    break;
                default:
                    throw new System.ArgumentException("Setting Type is Invalid", nameof(settingType));
            }

            return src.GetOrCreateSetting<T1>(settingName, defaultValue);
        }

        protected void SetSetting(string settingName, object value, EditorSettingManager.SettingType settingType)
        {
            EditorSetting dest = null;

            switch (settingType)
            {
                case EditorSettingManager.SettingType.ProjectSetting:
                    dest = inspectorProjectSettings;
                    break;
                case EditorSettingManager.SettingType.UserSetting:
                    dest = inspectorPreferenceSettings;
                    break;
                default:
                    throw new System.ArgumentException("Setting Type is Invalid", nameof(settingType));
            }

            dest.SetSettingValue(settingName, value);
        }
    }
}