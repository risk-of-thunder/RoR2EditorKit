using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RoR2EditorKit.Data
{
    /// <summary>
    /// Represents settings stored for an Editor to use.
    /// </summary>
    [Serializable]
    public class EditorSetting
    {
        /// <summary>
        /// The Type that owns these settings
        /// </summary>
        public Type OwnerType => Type.GetType(_editorTypeQualifiedName);
        /// <summary>
        /// A collection of all the settings stored in this EditorSetting
        /// </summary>
        public ReadOnlyCollection<string> AllSettingNames => _serializedSettings.Select(x => x.settingName).ToList().AsReadOnly();


        [SerializeField]
        internal string _typeName;
        [SerializeField]
        internal string _editorTypeQualifiedName;
        [SerializeField]
        internal int _editorTypeQualifiedHash;
        [SerializeField]
        internal List<SettingValue> _serializedSettings;

        /// <summary>
        /// Resets the setting specified in <paramref name="settingName"/> to it's default value
        /// <br>* If no setting of name <paramref name="settingName"/> exists, no setting is reset.</br>
        /// </summary>
        /// <param name="settingName">The setting to reset</param>
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

        /// <summary>
        /// Retrieves the value of the setting specified in <paramref name="settingName"/>
        /// <br>* If no setting with the name specified exists, a new one is created with the value specified in <paramref name="defaultValue"/></br>
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value to use in case the setting does not exist</param>
        /// <returns>The value for the setting</returns>
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

        /// <summary>
        /// Sets the value of the setting specified in <paramref name="settingName"/>
        /// <br>* If no setting with the name specified exists, a new one is created with the value specified in <paramref name="value"/></br>
        /// <br>* An <see cref="InvalidOperationException"/> gets thrown if a setting of name <paramref name="settingName"/> exists, but the ValueType of the setting is not the type of <paramref name="value"/></br>
        /// </summary>
        /// <param name="settingName">The name of the setting to set</param>
        /// <param name="value">The setting's new value</param>
        /// <exception cref="InvalidOperationException">Thrown then a setting with the name <paramref name="settingName"/> exists, but it's value type is not the type of <paramref name="value"/></exception>
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

        /// <summary>
        /// Orders the settings in the EditorSetting in alphabetical order.
        /// </summary>
        /// <param name="ignoredSettings">a series of settings that are taken out of the sort. the settings specified here gets sorted and then inserted at the beginning of the rest of the settings.</param>
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
