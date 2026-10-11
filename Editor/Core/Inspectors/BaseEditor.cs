using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2.Editor
{
    public abstract class BaseEditor<T> : UnityEditor.Editor where T : UnityEngine.Object
    {
        public EditorSettingCollection editorProjectSettings
        {
            get
            {
                return _editorProjectSettings ??= EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.ProjectSetting);
            }
        }
        private EditorSettingCollection _editorProjectSettings;

        public EditorSettingCollection editorPreferenceSettings
        {
            get
            {
                return _editorPreferenceSettings ??= EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.ProjectSetting);
            }
        }
        private EditorSettingCollection _editorPreferenceSettings;

        public bool editorEnabled
        {
            get => editorPreferenceSettings.GetOrCreateSetting("isInspectorEnabled", false);
            set
            {
                var origValue = editorPreferenceSettings.GetOrCreateSetting("isInspectorEnabled", false);
                if(origValue != value)
                {
                    editorPreferenceSettings.SetSettingValue("isInspectorEnabled", value);
                    OnEditorEnabledChangeInternal();
                }
            }
        }

        public T targetType => (T)target;
        public T[] targetTypes => (T[])targets;

        //stinky, bad, but whatever
        protected void ForceOnEditorEnabledChange() => OnEditorEnabledChangeInternal();
        
        private void OnEditorEnabledChangeInternal()
        {
            PreEditorEnabledCallbacks();

            if(editorEnabled)
            {
                OnEditorEnabled();
            }
            else
            {
                OnEditorDisabled();
            }

            PostEditorEnabledCallbacks();
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void PreEditorEnabledCallbacks() { }
        protected virtual void PostEditorEnabledCallbacks() { }

        protected virtual void OnEditorEnabled() { }
        protected virtual void OnEditorDisabled() { }

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {
            if (serializedObject.targetObject == null)
                return;

            serializedObject.ApplyModifiedProperties();
        }

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
                    src = editorProjectSettings;
                    break;
                case EditorSettingManager.SettingType.UserSetting:
                    src = editorPreferenceSettings;
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
                    dest = editorProjectSettings;
                    break;
                case EditorSettingManager.SettingType.UserSetting:
                    dest = editorPreferenceSettings;
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