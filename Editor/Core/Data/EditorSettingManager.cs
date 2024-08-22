using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;

namespace RoR2.Editor
{
    public static class EditorSettingManager
    {
        public static EditorSetting GetOrCreateSettingsFor(Type editorType, SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);

            var settings = GetEditorSetting(editorType, provider);
            if (settings != null)
            {
                return settings;
            }

            return CreateSettingsFor(editorType, settingType, provider);
        }

        public static void RemoveSetting(Type editorType, SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);

            var hash = editorType.AssemblyQualifiedName.GetHashCode();
            var settings = provider.editorSettings;

            for (int i = settings.Count - 1; i >= 0; i--)
            {
                var setting = settings[i];

                if (setting._editorTypeQualifiedHash != hash)
                    continue;

                settings.RemoveAt(i);
                provider.SaveSettings();
            }
        }

        public static void RemoveAllSettings(SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);

            provider.editorSettings.Clear();
            provider.SaveSettings();
        }

        public static void ResetSettings(Type editorType, SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);

            var setting = GetEditorSetting(editorType, provider);
            setting.ResetAllSettings();
            provider.SaveSettings();
        }

        public static void ResetAllSettings(SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);

            var settings = provider.editorSettings;

            for(int i = 0; i < settings.Count; i++)
            {
                var setting = settings[i];
                setting.ResetAllSettings();
            }
            provider.SaveSettings();
        }

        public static void PurgeOrphanedSettings(SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);

            var settings = provider.editorSettings;

            for(int i = settings.Count - 1; i >= 0; i--)
            {
                var setting = settings[i];

                if(setting.ownerType == null)
                {
                    Debug.Log($"Removing settings for type {setting._typeName} as these settings are orphaned.");
                    settings.RemoveAt(i);
                }
            }
            provider.SaveSettings();
        }

        public static IEditorSettingProvider GetEditorSettingProvider(SettingType settingType)
        {
            switch (settingType)
            {
                case SettingType.ProjectSetting:
                    return R2EKEditorProjectSettings.instance;
                case SettingType.UserSetting:
                    return R2EKEditorPreferenceSettings.instance;
                default:
                    throw new ArgumentException("Value for parameter is out of range", nameof(settingType));
            }
        }

        private static EditorSetting GetEditorSetting(Type editorType, IEditorSettingProvider provider)
        {
            var settings = provider.editorSettings;
            int hash = editorType.AssemblyQualifiedName.GetHashCode();
            for (int i = 0; i < settings.Count; i++)
            {
                var editorSetting = settings[i];
                if (editorSetting._editorTypeQualifiedHash != hash)
                    continue;

                return editorSetting;
            }

            return null;
        }

        private static EditorSetting CreateSettingsFor(Type editorType, SettingType settingType, IEditorSettingProvider provider)
        {
            var setting = new EditorSetting
            {
                _typeName = editorType.Name,
                _editorTypeQualifiedHash = editorType.AssemblyQualifiedName.GetHashCode(),
                _editorTypeQualifiedName = editorType.AssemblyQualifiedName,
                _serializedSettings = Array.Empty<EditorSetting.SettingValue>(),
                _settingType = settingType
            };
            provider.editorSettings.Add(setting);
            provider.SaveSettings();
            return setting;
        }

        public enum SettingType
        {
            ProjectSetting,
            UserSetting
        }

        public interface IEditorSettingProvider
        {
            List<EditorSetting> editorSettings { get; }
            void SaveSettings();
        }
    }
}