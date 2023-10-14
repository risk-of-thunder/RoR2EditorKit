using System.Collections.Generic;
using UnityEditor;
using ThunderKit.Core.Data;
using System;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2EditorKit.Data
{
    public sealed class EditorSettings : ThunderKitSetting
    {
        private ReadOnlyDictionary<Type, EditorSetting> TypeToSetting
        {
            get
            {
#pragma warning disable CS0618 // Type or member is obsolete
                if (__typeToSetting == null)
                    UpdateDictionary();
                return __typeToSetting;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
        [Obsolete("Use the property instead of the backing field")]
        private ReadOnlyDictionary<Type, EditorSetting> __typeToSetting;

        public bool enableNamingConventions = true;

        [SerializeField, ReadOnly]
        private List<SerializedEditorSettings> serializedSettings = new List<SerializedEditorSettings>();

        public T GetSettingValue<T>(Type type, string settingName)
        {
            return GetSettingValue<T>(type, settingName.GetHashCode());
        }

        public T GetSettingValue<T>(Type type, int hashSetting)
        {
            EditorSetting setting = GetEditorSetting(type);
            return setting.GetSettingValue<T>(hashSetting);
        }

        public EditorSetting GetEditorSetting(Type type)
        {
            if(!TypeToSetting.ContainsKey(type))
            {
                CreateSettingsFor(type);
            }
            return TypeToSetting[type];
        }

        public void CreateSettingsFor(Type type)
        {
            if(GetSerializedSetting(type.AssemblyQualifiedName, out var _) != -1)
            {
                return;
            }

            serializedSettings.Add(new SerializedEditorSettings
            {
                serializedSettings = new List<SerializedEditorSettings.SerializedSetting>(),
                editorTypeQualifiedName = type.AssemblyQualifiedName
            });
            UpdateDictionary();
        }

        public void SetSettingValue(object value, Type type, string settingName)
        {
            SetSettingValue(value, type, settingName.GetHashCode());
        }

        public void SetSettingValue(object value, Type type, int hashSetting)
        {
            int index = GetSerializedSetting(type.AssemblyQualifiedName, out SerializedEditorSettings serializedEditorSetting);

            if (index == -1)
            {
                CreateSettingsFor(type);
                SetSettingValue(value, type, hashSetting);
                return;
            }

            serializedEditorSetting.SetSetting(hashSetting, value);
            serializedSettings[index] = serializedEditorSetting;

            UpdateDictionary();
        }

        private int GetSerializedSetting(string assemblyQualifiedName, out SerializedEditorSettings serializedEditorSettings)
        {
            for(int i = 0; i < serializedSettings.Count; i++)
            {
                serializedEditorSettings = serializedSettings[i];
                if (serializedEditorSettings.editorTypeQualifiedName == assemblyQualifiedName)
                    return i;
            }
            serializedEditorSettings = default;
            return -1;
        }

        private void UpdateDictionary()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            __typeToSetting = null;
            Dictionary<Type, EditorSetting> typeToEditorSetting = new Dictionary<Type, EditorSetting>();

            foreach(var serializedEditorSetting in serializedSettings)
            {
                EditorSetting setting = serializedEditorSetting;
                typeToEditorSetting.Add(setting.ownerType, setting);
            }
            __typeToSetting = new ReadOnlyDictionary<Type, EditorSetting>(typeToEditorSetting);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}