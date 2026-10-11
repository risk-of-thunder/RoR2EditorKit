using System;
using System.ComponentModel;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.Deprecated
{
    [Obsolete(OBSOLETE_MESSAGE)]
    public abstract class Inspector<T> : UnityEditor.Editor where T : UnityEngine.Object
    {
        public const string OBSOLETE_MESSAGE = "The \"Inspector\" framework for editors is deprecated, utilize \"BaseEditor\" instead.";

        public EditorSettingCollection inspectorProjectSettings
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
        private EditorSettingCollection _inspectorProjectSettings;

        public EditorSettingCollection inspectorPreferenceSettings
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
        private EditorSettingCollection _inspectorPreferenceSettings;

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

        public T targetType => (T)target;

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
                OnInspectorDisabled();
            }
            else
            {
                var inspectorElement = CreateInspectorUI();
                rootVisualElement.Add(inspectorElement);
                if (hasDoneFirstDrawing)
                    rootVisualElement.Bind(serializedObject);
                OnInspectorEnabled();
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

        protected virtual void OnInspectorEnabled() { }

        protected virtual void OnInspectorDisabled() { }

        protected virtual void OnEnable()
        {
            OnInspectorEnabled();
        }

        protected virtual void OnDisable()
        {
            OnInspectorDisabled();
            if (serializedObject.targetObject == null)
                return;

            serializedObject.ApplyModifiedProperties();
        }

        protected abstract VisualElement CreateInspectorUI();

        protected T1 GetOrCreateSetting<T1>(string settingName, T1 defaultValue, EditorSettingManager.SettingType settingType)
        {

            EditorSettingCollection src = null;
            switch (settingType)
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

        protected T1 GetOrCreateSetting<T1>(string settingName, T1 defaultValue, string providerName)
        {
            var provider = EditorSettingManager.GetEditorSettingProvider(providerName);

            var settings = EditorSettingManager.GetOrCreateSettingsFor(GetType(), provider);

            return settings.GetOrCreateSetting(settingName, defaultValue);
        }

        protected void SetSetting(string settingName, object value, EditorSettingManager.SettingType settingType)
        {
            EditorSettingCollection dest = null;

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

        protected void SetSetting(string settingName, object value, string providerName)
        {
            var provider = EditorSettingManager.GetEditorSettingProvider(providerName);

            var settings = EditorSettingManager.GetOrCreateSettingsFor(GetType(), provider);

            settings.SetSettingValue(settingName, value);
        }
    }
}