using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoR2EditorKit.Data
{
    [Serializable]
    public class EditorSetting
    {
        public Type OwnerType => Type.GetType(_editorTypeQualifiedName);
        public ReadOnlyCollection<string> AllSettingNames => _serializedSettings.Select(x => x.settingName).ToList().AsReadOnly();

        [SerializeField]
        internal string _typeName;
        [SerializeField]
        internal string _editorTypeQualifiedName;
        [SerializeField]
        internal int _editorTypeQualifiedHash;
        [SerializeField]
        internal List<SettingValue> _serializedSettings;

        public void ResetSetting(string settingName)
        {
            int id = settingName.GetHashCode();
            foreach(var setting in _serializedSettings)
            {
                if(setting.settingID != id)
                {
                    continue;
                }
                setting.value = setting.defaultValue;
                return;
            }
        }

        public T GetSetting<T>(string settingName, T defaultValue = default)
        {
            int id = settingName.GetHashCode();
            foreach(var setting in _serializedSettings)
            {
                if(setting.settingID != id)
                {
                    continue;
                }
                return (T)EditorStringSerializer.Deserialize(setting.value, Type.GetType(setting.valueTypeQualifiedName));
            }
            return (T)CreateSetting(defaultValue, settingName);
        }

        public void SetSetting(string settingName, object value)
        {
            int id = settingName.GetHashCode();
            Type type = value.GetType();
            foreach(var setting in _serializedSettings)
            {
                if(setting.settingID != id)
                {
                    continue;
                }

                if(setting.valueTypeQualifiedName != type.AssemblyQualifiedName)
                {
                    throw new InvalidOperationException($"Cannot replace setting {settingName}'s ValueType. (Setting Value Type: {Type.GetType(setting.valueTypeQualifiedName).FullName}, attempted asignment type: {type.FullName})");
                }

                setting.value = EditorStringSerializer.Serialize(value, type);
            }
            CreateSetting(value, settingName);
        }

        public void OrderSettings(params string[] ignoredSettings)
        {
            int[] ids = ignoredSettings.Select(x => x.GetHashCode()).ToArray();
            List<SettingValue> settings = new List<SettingValue>();
            foreach(var setting in _serializedSettings)
            {
                if(ids.Contains(setting.settingID))
                {
                    settings.Add(setting);
                }
            }
            foreach(var setting in settings)
            {
                _serializedSettings.Remove(setting);
            }
            settings.Sort((x, y) => x.settingName.CompareTo(y.settingName));
            _serializedSettings.Sort((x, y) => x.settingName.CompareTo(y.settingName));
            _serializedSettings.InsertRange(0, settings);
        }

        private object CreateSetting(object value, string settingName)
        {
            Type valueType = value.GetType();
            var serializedValue = EditorStringSerializer.Serialize(value, value.GetType());
            _serializedSettings.Add(new SettingValue
            {
                defaultValue = serializedValue,
                value = serializedValue,
                settingID = settingName.GetHashCode(),
                settingName = settingName,
                valueTypeQualifiedName = valueType.AssemblyQualifiedName
            });
            OrderSettings("IsEditorEnabled");
            return value;
        }

        [Serializable]
        internal class SettingValue
        {
            [SerializeField]
            internal string settingName;
            [SerializeField]
            internal int settingID;

            [SerializeField]
            internal string valueTypeQualifiedName;
            [SerializeField]
            internal string value;
            [SerializeField]
            internal string defaultValue;
        }
    }
}
