using System;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// An <see cref="EditorSettingCollection"/> represents an object that stores multiple types of Settings that are tied to a specific type.
    /// 
    /// <para>
    /// EditorSettings can be used to store either project settings or user preference settings, Examples include storing Colors that can be later used on a custom inspector to draw custom Gizmos/Handles of the user's preference colors.
    /// </para>
    /// 
    /// <br>See also <see cref="EditorSettingManager"/></br>
    /// </summary>
    [Serializable]
    public class EditorSettingCollection
    {
        /// <summary>
        /// Returns the Type that owns this instance of <see cref="EditorSettingCollection"/>.
        /// </summary>
        public Type ownerType => Type.GetType(_editorTypeQualifiedName);

        /// <summary>
        /// Returns all the setting names that this EditorSetting contains
        /// </summary>
        public ReadOnlyCollection<string> allSettingNames => _serializedSettings.Select(x => x._settingName).ToList().AsReadOnly();

        [SerializeField]
        internal string _typeName;
        [SerializeField]
        internal string _editorTypeQualifiedName;
        [SerializeField]
        internal int _editorTypeQualifiedHash;
        [SerializeField]
        internal SettingValue[] _serializedSettings;

        /// <summary>
        /// Returns the type of EditorSetting, which can be used to obtain it's provider.
        ///
        /// <br>If the setting type is of value <see cref="EditorSettingManager.SettingType.Custom"/>, you can utilize the <see cref="settingProviderName"/> to obtain the provider</br>
        /// </summary>
        public EditorSettingManager.SettingType settingType => _settingType;
        [SerializeField]
        internal EditorSettingManager.SettingType _settingType;

        /// <summary>
        /// Represents the name of the provider that created this EditorSetting, the provider can be obtained utilizing <see cref="EditorSettingManager.GetEditorSettingProvider(string)"/> and passing this string.
        /// </summary>
        public string settingProviderName => _settingProviderName;
        [SerializeField]
        internal string _settingProviderName;

        /// <summary>
        /// Saves the settings to the disk
        /// </summary>
        public void SaveSettings()
        {
            if (_settingType == EditorSettingManager.SettingType.Custom)
            {
                EditorSettingManager.GetEditorSettingProvider(_settingProviderName).SaveSettings();
                return;
            }
            EditorSettingManager.GetEditorSettingProvider(_settingType).SaveSettings();
        }

        /// <summary>
        /// Resets all the settings stored by this EditorSetting to their default values.
        /// </summary>
        public void ResetAllSettings()
        {
            for (int i = 0; i < _serializedSettings.Length; i++)
            {
                ref SettingValue setting = ref _serializedSettings[i];
                setting.ResetValue();
            }
        }

        /// <summary>
        /// Resets the specified setting in <paramref name="settingName"/> to it's default value
        /// </summary>
        /// <param name="settingName">The setting to reset</param>
        public void ResetSetting(string settingName)
        {
            int id = settingName.GetHashCode();

            for (int i = 0; i < _serializedSettings.Length; i++)
            {
                ref var setting = ref _serializedSettings[i];
                if (setting._settingID != id)
                    continue;

                setting.ResetValue();
                break;
            }
        }

        /// <summary>
        /// Gets a the value of a Setting of name <paramref name="settingName"/>. if no setting with that name exists, it's created using <paramref name="defaultValue"/> as it's default value
        /// </summary>
        /// <typeparam name="T">The data type of the setting's value.</typeparam>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value of this setting, used when the setting is being created</param>
        /// <returns>The requested setting</returns>
        public T GetOrCreateSetting<T>(string settingName, T defaultValue = default)
        {
            int id = settingName.GetHashCode();
            for (int i = 0; i < _serializedSettings.Length; i++)
            {
                ref var setting = ref _serializedSettings[i];
                if (setting._settingID != id)
                    continue;

                return (T)setting.boxedValue;
            }

            return (T)CreateSetting(defaultValue, settingName);
        }

        /// <summary>
        /// Sets the value of a Setting of name <paramref name="settingName"/> using the value in <paramref name="newValue"/>.
        /// <br>No changes are made if the setting doesnt exists.</br>
        /// </summary>
        /// <param name="settingName">The setting to change it's value</param>
        /// <param name="newValue">The new value for the setting</param>
        public void SetSettingValue(string settingName, object newValue)
        {
            int id = settingName.GetHashCode();
            for (int i = 0; i < _serializedSettings.Length; i++)
            {
                ref var setting = ref _serializedSettings[i];
                if (setting._settingID != id)
                    continue;

                if (setting.boxedValue != newValue)
                {
                    setting.boxedValue = newValue;
                    SaveSettings();
                }
                return;
            }

            Debug.Log($"No EditorSetting of name {settingName} could be found, Create the setting first using \"GetOrCreateSetting<T>(string, T)\"");
        }

        private object CreateSetting(object defaultValueForSetting, string settingName)
        {
            ArrayUtility.Add(ref _serializedSettings, new SettingValue
            {
                boxedValue = defaultValueForSetting,
                _settingID = settingName.GetHashCode(),
                _settingName = settingName,
            });
            SaveSettings();
            return defaultValueForSetting;
        }

        [Serializable]
        internal struct SettingValue
        {
            public object boxedValue
            {
                get
                {
                    //Ensure boxed value is deserialized prior to comparing equality.
                    if (_boxedValue == null && _valueTypeQualifiedName != null)
                        Deserialize();

                    return _boxedValue;
                }
                set
                {
                    //Ensure boxed value is deserialized prior to comparing equality.
                    if (_boxedValue == null && _valueTypeQualifiedName != null)
                        Deserialize();

                    if (_boxedValue != value)
                    {
                        _boxedValue = value;
                        Serialize();
                    }
                }
            }
            private object _boxedValue;
            [SerializeField]
            internal string _settingName;
            [SerializeField]
            internal int _settingID;

            public Type settingType => Type.GetType(_valueTypeQualifiedName);
            [SerializeField]
            private string _valueTypeQualifiedName;
            [SerializeField]
            private string _serializedValue;
            [SerializeField]
            private string _defaultSerializedValue;

            private void Deserialize()
            {
                var type = Type.GetType(_valueTypeQualifiedName);
                _boxedValue = EditorStringSerializer.Deserialize(type, _serializedValue);
            }

            private void Serialize()
            {
                var type = _boxedValue.GetType();
                _valueTypeQualifiedName = type.AssemblyQualifiedName;
                _serializedValue = EditorStringSerializer.Serialize(type, _boxedValue);
            }

            internal void ResetValue()
            {
                _serializedValue = _defaultSerializedValue;
                Deserialize();
            }
        }
    }
}