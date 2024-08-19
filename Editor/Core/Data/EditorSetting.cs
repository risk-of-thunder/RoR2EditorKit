using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace RoR2.Editor
{
    [Serializable]
    public class EditorSetting
    {
        public Type ownerType => Type.GetType(_editorTypeQualifiedName);

        public ReadOnlyCollection<string> allSettingNames => _serializedSettings.Select(x => x._settingName).ToList().AsReadOnly();

        [SerializeField]
        internal string _typeName;
        [SerializeField]
        internal string _editorTypeQualifiedName;
        [SerializeField]
        internal int _editorTypeQualifiedHash;
        [SerializeField]
        internal List<SettingValue> _serializedSettings;
        [SerializeField]
        internal EditorSettingManager.SettingType _settingType;

        public void SaveSettings()
        {
            EditorSettingManager.GetEditorSettingProvider(_settingType).SaveSettings();
        }

        public void ResetAllSettings()
        {
            for(int i = 0; i < _serializedSettings.Count; i++)
            {
                var setting = _serializedSettings[i];
                setting.ResetValue();
                _serializedSettings[i] = setting;
            }
        }

        public void ResetSetting(string settingName)
        {
            int id = settingName.GetHashCode();

            for(int i = 0; i < _serializedSettings.Count; i++)
            {
                var setting = _serializedSettings[i];
                if (setting._settingID != id)
                    continue;

                setting.ResetValue();
                _serializedSettings[i] = setting;
                break;
            }
        }

        public T GetOrCreateSetting<T>(string settingName, T defaultValue = default)
        {
            int id = settingName.GetHashCode();
            for(int i = 0; i < _serializedSettings.Count; i++)
            {
                var setting = _serializedSettings[i];
                if (setting._settingID != id)
                    continue;

                return (T)setting.boxedValue;
            }

            return (T)CreateSetting(defaultValue, settingName);
        }

        public void SetSettingValue(string settingName, object newValue)
        {
            int id = settingName.GetHashCode();
            Type type = newValue.GetType();
            for(int i = 0; i < _serializedSettings.Count; i++)
            {
                var setting = _serializedSettings[i];
                if (setting._settingID != id)
                    continue;

                setting.boxedValue = newValue;
                return;
            }

            Debug.Log($"No EditorSetting of name {settingName} could be found, Create the setting first using \"GetOrCreateSetting<T>(string, T)\"");
        }

        private object CreateSetting(object defaultValueForSetting, string settingName)
        {
            Type valueType = defaultValueForSetting.GetType();
            _serializedSettings.Add(new SettingValue
            {
                boxedValue = defaultValueForSetting,
                _settingID = settingName.GetHashCode(),
                _settingName = settingName,
            });
            return defaultValueForSetting;
        }

        [Serializable]
        internal struct SettingValue
        {
            internal object boxedValue
            {
                get
                {
                    //Ensure boxed value is deserialized prior to comparing equality.
                    if (_boxedValue == null)
                        Deserialize();

                    return _boxedValue;
                }
                set
                {
                    //Ensure boxed value is deserialized prior to comparing equality.
                    if (_boxedValue == null)
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