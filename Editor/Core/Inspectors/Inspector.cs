using System;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="Inspector{T}"/> is the base <see cref="UnityEditor.Editor"/> for all the R2EK inspectors.
    /// <br>These inspectors utilize the UIElements sytem, however, IMGUI can also be used by utilizing the <see cref="IMGUIContainer"/></br>
    /// <para>These inspectors are extremely configurable by the end user by the usage of the <see cref="EditorSettingCollection"/> system. alongside this, these inspectors can be disabled or enabled, which is saved as a user preference.</para>
    /// <br>If you desire to make an inspector for a <see cref="ScriptableObject"/>, look into the <see cref="ScriptableObjectInspector{T}"/></br>
    /// <br>If you desire to make an inspector for a <see cref="ComponentInspector{T}"/>, look into the <see cref="ComponentInspector{T}"/></br>
    /// </summary>
    /// <typeparam name="T">The type of Object being inspected</typeparam>
    public abstract class Inspector<T> : UnityEditor.Editor where T : UnityEngine.Object
    {
        /// <summary>
        /// Returns the Inspector's ProjectSettings's EditorSetting
        /// </summary>
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

        /// <summary>
        /// Returns the Inspector's UserSetting's EditorSetting
        /// </summary>
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

        /// <summary>
        /// Checks wether the inspector is enabled or disabled, disabled inspectors display their default inspector UI
        /// </summary>
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

        /// <summary>
        /// The root visual element of this inspector, custom controls should be added to this VisualElement, as this is returned by <see cref="CreateInspectorGUI"/>
        /// </summary>
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

        /// <summary>
        /// Direct access to the serializedObject's targetObject, casted to <typeparamref name="T"/>
        /// </summary>
        public T targetType => (T)target;

        /// <summary>
        /// Checks if the inspector has done it's first drawing call.
        /// </summary>
        protected bool hasDoneFirstDrawing { get; private set; }

        /// <summary>
        /// Event fired when <see cref="rootVisualElement"/> is cleared. This can be used to add elements that are displayed regardless if the inspector is enabled or disabled
        /// </summary>
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

        /// <summary>
        /// Base method that returns the VisualElement for this UI, you cannot override this.
        /// Instead, implement <see cref="CreateInspectorUI"/>, this method may be abstracted further by types that inherit from <see cref="Inspector{T}"/>, such as the case with <see cref="IMGUIScriptableObjectInspector{T}"/> or <see cref="VisualElementComponentInspector{T}"/>
        /// </summary>
        public sealed override VisualElement CreateInspectorGUI()
        {
            serializedObject.Update();
            OnInspectorEnabledChange();
            serializedObject.ApplyModifiedProperties();
            hasDoneFirstDrawing = true;
            return rootVisualElement;
        }

        /// <summary>
        /// Method fired when the custom inspector becomes enabled
        /// </summary>
        protected virtual void OnInspectorEnabled() { }

        /// <summary>
        /// Method fired when the custom inspector becomes disabled
        /// </summary>
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

        /// <summary>
        /// Implement your custom UI here, this method may be sealed and abstracted away by other classes.
        /// </summary>
        /// <returns>The custom VisualElement that'll be displayed when the inspector is enabled</returns>
        protected abstract VisualElement CreateInspectorUI();

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/>
        /// 
        /// <para>Keep in mind that the value used in <paramref name="settingType"/> will make it so either the <see cref="EditorSettingCollection>"/> from <see cref="inspectorProjectSettings"/> or <see cref="inspectorPreferenceSettings"/> is used. If you want to store it in a different provider, utilize <see cref="GetOrCreateSetting{T1}(string, T1, string)"/></para>
        /// </summary>
        /// <typeparam name="T1"><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></typeparam>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value of this setting, used when the setting is being created</param>
        /// <param name="settingType">The type of the setting, this cannot be <see cref="EditorSettingManager.SettingType.Custom"/></param>
        /// <returns><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></returns>
        /// <exception cref="System.ArgumentException"></exception>
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

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/> 
        /// <typeparam name="T1"><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></typeparam>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value of this setting, used when the setting is being created</param>
        /// <param name="providerName">The name of the provider that'll store the setting.</param>
        /// <returns><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></returns>
        protected T1 GetOrCreateSetting<T1>(string settingName, T1 defaultValue, string providerName)
        {
            var provider = EditorSettingManager.GetEditorSettingProvider(providerName);

            var settings = EditorSettingManager.GetOrCreateSettingsFor(GetType(), provider);

            return settings.GetOrCreateSetting(settingName, defaultValue);
        }


        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.SetSettingValue(string, object)"/>
        /// 
        /// <para>Keep in mind that the value used in <paramref name="settingType"/> will make it so either the <see cref="EditorSettingCollection>"/> from <see cref="inspectorProjectSettings"/> or <see cref="inspectorPreferenceSettings"/> is used. If you want to store it in a different provider, utilize <see cref="GetOrCreateSetting{T1}(string, T1, string)"/></para>
        /// </summary>
        /// <param name="settingName">The setting to change it's value</param>
        /// <param name="value">The new value for the setting</param>
        /// <param name="settingType">The type of the setting, this cannot be <see cref="EditorSettingManager.SettingType.Custom"/></param>
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

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.SetSettingValue(string, object)"/>
        /// </summary>
        /// <param name="settingName">The setting to change it's value</param>
        /// <param name="value">The new value for the setting</param>
        /// <param name="providerName">The name of the provider that'll store the setting.</param>
        protected void SetSetting(string settingName, object value, string providerName)
        {
            var provider = EditorSettingManager.GetEditorSettingProvider(providerName);

            var settings = EditorSettingManager.GetOrCreateSettingsFor(GetType(), provider);

            settings.SetSettingValue(settingName, value);
        }
    }
}