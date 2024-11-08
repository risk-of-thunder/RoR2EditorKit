using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// An <see cref="ExtendedEditorWindow"/> is an EditorWindow that contains extended functionality utlizing R2EK's Systems. Main features include automatic serializedObject creation alongside properties for accessing <see cref="EditorSettingCollection"/>s tied to the window
    /// </summary>
    public abstract class ExtendedEditorWindow : EditorWindow
    {
        /// <summary>
        /// Returns the Window's ProjectSetting's EditorSetting
        /// </summary>
        public EditorSettingCollection windowProjectSettings
        {
            get
            {
                if (_windowProjectSettings == null)
                {
                    _windowProjectSettings = EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.ProjectSetting);
                }
                return _windowProjectSettings;
            }
        }
        private EditorSettingCollection _windowProjectSettings;

        /// <summary>
        /// Returns the Window's UserSetting's EditorSetting
        /// </summary>
        public EditorSettingCollection windowPreferenceSettings
        {
            get
            {
                if (_windowPreferenceSettings == null)
                {
                    _windowPreferenceSettings = EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.UserSetting);
                }
                return _windowPreferenceSettings;
            }
        }
        private EditorSettingCollection _windowPreferenceSettings;

        /// <summary>
        /// The current <see cref="SerializedObject"/> for this window
        /// </summary>
        protected SerializedObject serializedObject
        {
            get
            {
                return _serializedObject;
            }
            set
            {
                if (_serializedObject != value)
                {
                    _serializedObject = value;
                    OnSerializedObjectChanged();
                    return;
                }
            }
        }
        private SerializedObject _serializedObject;

        /// <summary>
        /// The source object from which <see cref="_serializedObject"/> should be created
        /// </summary>
        [SerializeField]
        protected UnityEngine.Object _sourceSerializedObject;

        /// <summary>
        /// Opens an ExtendedEditorWindow
        /// </summary>
        /// <typeparam name="TEditorWindow">The type of the EditorWindow</typeparam>
        /// <param name="serializedObjectOverride">The object that will act as the object to wrap with a SerializedObject, if left null, the window instance is used.</param>
        /// <returns>The instance of the window</returns>
        public static TEditorWindow Open<TEditorWindow>(UnityEngine.Object serializedObjectOverride = null) where TEditorWindow : ExtendedEditorWindow
        {
            TEditorWindow window = GetWindow<TEditorWindow>();
            window._sourceSerializedObject = serializedObjectOverride ?? window;
            return window;
        }

        /// <summary>
        /// Method raised when <see cref="serializedObject"/> changes
        /// </summary>
        protected virtual void OnSerializedObjectChanged() { }

        protected abstract void CreateGUI();

        protected virtual void OnEnable() { }

        protected virtual void OnDisable() { }

        protected virtual void Awake() { }

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/>
        /// 
        /// <para>Keep in mind that the value used in <paramref name="settingType"/> will make it so either the <see cref="EditorSettingCollection>"/> from <see cref="windowProjectSettings"/> or <see cref="windowPreferenceSettings"/> is used. If you want to store it in a different provider, utilize <see cref="GetOrCreateSetting{T1}(string, T1, string)"/></para>
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
                    src = windowProjectSettings;
                    break;
                case EditorSettingManager.SettingType.UserSetting:
                    src = windowPreferenceSettings;
                    break;
                default:
                    throw new System.ArgumentException("Setting Type is Invalid", nameof(settingType));
            }

            return src.GetOrCreateSetting<T1>(settingName, defaultValue);
        }

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/> 
        /// <para>Keep in mind that the value used in <paramref name="settingType"/> will make it so either the <see cref="EditorSettingCollection>"/> from <see cref="windowProjectSettings"/> or <see cref="windowPreferenceSettings"/> is used. If you want to store it in a different provider, utilize <see cref="GetOrCreateSetting{T1}(string, T1, string)"/></para>
        /// </summary>
        /// <typeparam name="T1"><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></typeparam>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value of this setting, used when the setting is being created</param>
        /// <param name="providerName">The name of the provider that'll store the setting.</param>
        /// <returns><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></returns>
        /// <exception cref="System.ArgumentException"></exception>
        protected T1 GetOrCreateSetting<T1>(string settingName, T1 defaultValue, string providerName)
        {
            var provider = EditorSettingManager.GetEditorSettingProvider(providerName);

            var settings = EditorSettingManager.GetOrCreateSettingsFor(GetType(), provider);

            return settings.GetOrCreateSetting(settingName, defaultValue);
        }

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.SetSettingValue(string, object)"/>
        /// <para>Keep in mind that the value used in <paramref name="settingType"/> will make it so either the <see cref="EditorSettingCollection>"/> from <see cref="windowProjectSettings"/> or <see cref="windowPreferenceSettings"/> is used. If you want to store it in a different provider, utilize <see cref="GetOrCreateSetting{T1}(string, T1, string)"/></para>
        /// 
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
                    dest = windowProjectSettings;
                    break;
                case EditorSettingManager.SettingType.UserSetting:
                    dest = windowPreferenceSettings;
                    break;
                default:
                    throw new System.ArgumentException("Setting Type is Invalid", nameof(settingType));
            }

            dest.SetSettingValue(settingName, value);
        }

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.SetSettingValue(string, object)"/>
        /// <para>Keep in mind that the value used in <paramref name="settingType"/> will make it so either the <see cref="EditorSettingCollection>"/> from <see cref="windowProjectSettings"/> or <see cref="windowPreferenceSettings"/> is used. If you want to store it in a different provider, utilize <see cref="GetOrCreateSetting{T1}(string, T1, string)"/></para>
        /// 
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
