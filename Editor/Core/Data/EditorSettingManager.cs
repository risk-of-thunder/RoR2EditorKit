using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// The EditorSettingManager is a static class that allows for the ease of management of <see cref="EditorSettingCollection"/> stored within <see cref="IEditorSettingProvider"/>s.
    /// <br>By default, RoR2EditorKit provides both management of EditorSettings for Projects, and EditorSettings for user preferences, these can be chosen in the methods that accept <see cref="SettingType"/> as an argument.</br>
    /// <para>New setting providers can be registered to the manager with the use of <see cref="RegisterProvider(IEditorSettingProvider, Func{IEditorSettingProvider})"/></para>
    /// <br>For more info regarding the EditorSetting system, see <see cref="EditorSettingCollection"/>'s documentation</br>
    /// </summary>
    public static class EditorSettingManager
    {
        private static Dictionary<string, Func<IEditorSettingProvider>> _nameToProvider = new Dictionary<string, Func<IEditorSettingProvider>>();

        /// <summary>
        /// Gets a <see cref="EditorSettingCollection"/> for the type specified in <paramref name="editorType"/>, using one of the default providers provided by RoR2EK. if the <see cref="EditorSettingCollection"/> does not exists, a new one is created.
        /// 
        /// <br>See also <see cref="GetOrCreateSettingsFor(Type, IEditorSettingProvider)"/></br>
        /// </summary>
        /// <param name="editorType">The System.Type that owns the setting</param>
        /// <param name="settingType">The type of setting, this cannot be <see cref="SettingType.Custom"/></param>
        /// <returns>The EditorSetting</returns>
        public static EditorSettingCollection GetOrCreateSettingsFor(Type editorType, SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);

            var settings = GetEditorSetting(editorType, provider);
            if (settings != null)
            {
                return settings;
            }

            return CreateSettingsFor(editorType, settingType, provider);
        }

        /// <summary>
        /// Gets a <see cref="EditorSettingCollection"/> for the type specified in <paramref name="editorType"/>, using the specified <paramref name="provider"/>. If the <see cref="EditorSettingCollection"/> does not exist, a new one is created.
        /// </summary>
        /// <param name="editorType">The System.Type that owns the setting</param>
        /// <param name="provider">The provider that's storing the setting</param>
        /// <returns>The EditorSetting</returns>
        public static EditorSettingCollection GetOrCreateSettingsFor(Type editorType, IEditorSettingProvider provider)
        {
            var settings = GetEditorSetting(editorType, provider);
            if (settings != null)
            {
                return settings;
            }

            return CreateSettingsFor(editorType, SettingType.Custom, provider);
        }

        /// <summary>
        /// Completely removes the EditorSetting that's tied to <paramref name="editorType"/>, and is stored by one of the default providers from R2EK.
        /// 
        /// <para>See also <see cref="RemoveSetting(Type, IEditorSettingProvider)"/></para>
        /// </summary>
        /// <param name="editorType">The System.Type of the EditorSetting to Remove</param>
        /// <param name="settingType">The type of setting, this cannot be <see cref="SettingType.Custom"/></param>
        public static void RemoveSetting(Type editorType, SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);
            RemoveSetting(editorType, provider);
        }

        /// <summary>
        /// Compeltely removes the EditorSetting that's tied to <paramref name="editorType"/> and is stored by the specified <paramref name="provider"/>.
        /// </summary>
        /// <param name="editorType">The System.Type of the EditorSetting to Remove</param>
        /// <param name="provider">The provider that's storing the setting</param>
        public static void RemoveSetting(Type editorType, IEditorSettingProvider provider)
        {
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

        /// <summary>
        /// Completely removes all the EditorSettings that are stored within one of R2EK's default providers
        /// </summary>
        /// <param name="settingType">The type of setting to clear, this cannot be <see cref="SettingType.Custom"/></param>
        public static void RemoveAllSettings(SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);
            RemoveAllSettings(provider);
        }

        /// <summary>
        /// Completely removes all the EditorSettings that are stored within the specified <paramref name="provider"/>
        /// </summary>
        /// <param name="provider">The provider to clear</param>
        public static void RemoveAllSettings(IEditorSettingProvider provider)
        {
            provider.editorSettings.Clear();
            provider.SaveSettings();
        }

        /// <summary>
        /// Resets the settings back to their default values that are tied to the <paramref name="editorType"/>, and is stored by one of the default providers from R2EK
        /// </summary>
        /// <param name="editorType">The System.Type of the EditorSetting to Reset</param>
        /// <param name="settingType">The type of setting to clear, this cannot be <see cref="SettingType.Custom"/></param>
        public static void ResetSettings(Type editorType, SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);
            ResetSettings(editorType, provider);
        }

        /// <summary>
        /// Resets the settings back to their default values that are tied to the <paramref name="editorType"/>, and are stored within the specified <paramref name="provider"/>
        /// </summary>
        /// <param name="editorType">The System.Type of the EditorSetting to reset</param>
        /// <param name="provider">The provider that contains the Editorsetting</param>
        public static void ResetSettings(Type editorType, IEditorSettingProvider provider)
        {
            var setting = GetEditorSetting(editorType, provider);
            setting.ResetAllSettings();
            provider.SaveSettings();
        }

        /// <summary>
        /// Resets all the setting's values back to their default values from one of the default providers from R2EK
        /// </summary>
        /// <param name="settingType">The type of settings to reset, this cannot be <see cref="SettingType.Custom"/></param>
        public static void ResetAllSettings(SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);
            ResetAllSettings(provider);
        }

        /// <summary>
        /// Resets all the setting's values back to their default values from the specified <paramref name="provider"/>
        /// </summary>
        /// <param name="provider">The provider to reset it's settings</param>
        public static void ResetAllSettings(IEditorSettingProvider provider)
        {
            var settings = provider.editorSettings;

            for (int i = 0; i < settings.Count; i++)
            {
                var setting = settings[i];
                setting.ResetAllSettings();
            }
            provider.SaveSettings();
        }

        /// <summary>
        /// Purges all orphaned settings from one of the default providers from R2EK.
        /// 
        /// <para>Orphaned settings are <see cref="EditorSettingCollection"/> that have no Owners, this is checked if the EditorSetting's <see cref="EditorSettingCollection.ownerType"/> returns null.</para>
        /// </summary>
        /// <param name="settingType">The type of setting to purge of orphaned settings, this cannot be <see cref="SettingType.Custom"/></param>
        public static void PurgeOrphanedSettings(SettingType settingType)
        {
            IEditorSettingProvider provider = GetEditorSettingProvider(settingType);
            PurgeOrphanedSettings(provider);
        }

        /// <summary>
        /// Purges all orphaned settings from the specified <paramref name="provider"/>
        /// 
        /// <para>Orphaned settings are <see cref="EditorSettingCollection"/> that have no Owners, this is checked if the EditorSetting's <see cref="EditorSettingCollection.ownerType"/> returns null.</para>
        /// </summary>
        /// <param name="provider">The provider to purge of orphaned settings</param>
        public static void PurgeOrphanedSettings(IEditorSettingProvider provider)
        {
            var settings = provider.editorSettings;

            for (int i = settings.Count - 1; i >= 0; i--)
            {
                var setting = settings[i];

                if (setting.ownerType == null)
                {
                    Debug.Log($"Removing settings for type {setting._typeName} as these settings are orphaned.");
                    settings.RemoveAt(i);
                }
            }
            provider.SaveSettings();
        }

        /// <summary>
        /// Returns the <see cref="IEditorSettingProvider"/> that's tied to <paramref name="settingType"/>. these providers are ones that R2EK provides by default.
        /// </summary>
        /// <param name="settingType">The type of provider to obtain</param>
        /// <returns>The provider</returns>
        /// <exception cref="ArgumentException">Thrown when the enum is not a valid value, or if the value of <paramref name="settingType"/> is <see cref="SettingType.Custom"/></exception>
        public static IEditorSettingProvider GetEditorSettingProvider(SettingType settingType)
        {
            switch (settingType)
            {
                case SettingType.ProjectSetting:
                    return R2EKEditorProjectSettings.instance;
                case SettingType.UserSetting:
                    return R2EKEditorPreferenceSettings.instance;
                case SettingType.Custom:
                    throw new ArgumentException("Obtaining the provider of a setting type which is Custom is not allowed. Obtain the provider by string instead.");
                default:
                    throw new ArgumentException("Value for parameter is out of range", nameof(settingType));
            }
        }

        /// <summary>
        /// Returns the <see cref="IEditorSettingProvider"/> of a specific name. The EditorSettingProvider must be registered first into the system for the manager to find it.
        /// </summary>
        /// <param name="name">The name of the provider</param>
        /// <returns>The provider</returns>
        /// <exception cref="KeyNotFoundException">The specified name is not registered in the EditorSettingManager</exception>
        public static IEditorSettingProvider GetEditorSettingProvider(string name)
        {
            if (!_nameToProvider.ContainsKey(name))
            {
                throw new KeyNotFoundException($"Provider with name {name} is not registered");
            }
            return _nameToProvider[name]();
        }

        /// <summary>
        /// Registers a new <see cref="IEditorSettingProvider"/> to the EditorSettingManager
        /// </summary>
        /// <param name="provider">An instance of the provider to register, this is used to obtain the provider's providerName.</param>
        /// <param name="providerCreator">A Function that returns an instance of the provider.</param>
        /// <returns>True if the provider was registered succesfully, otherwise false</returns>
        public static bool RegisterProvider(IEditorSettingProvider provider, Func<IEditorSettingProvider> providerCreator)
        {
            if (_nameToProvider.ContainsKey(provider.providerName))
            {
                return false;
            }

            _nameToProvider[provider.providerName] = providerCreator;
            return true;
        }

        private static EditorSettingCollection GetEditorSetting(Type editorType, IEditorSettingProvider provider)
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

        private static EditorSettingCollection CreateSettingsFor(Type editorType, SettingType settingType, IEditorSettingProvider provider)
        {
            var setting = new EditorSettingCollection
            {
                _typeName = editorType.Name,
                _editorTypeQualifiedHash = editorType.AssemblyQualifiedName.GetHashCode(),
                _editorTypeQualifiedName = editorType.AssemblyQualifiedName,
                _serializedSettings = Array.Empty<EditorSettingCollection.SettingValue>(),
                _settingType = settingType,
                _settingProviderName = provider.providerName
            };
            provider.editorSettings.Add(setting);
            provider.SaveSettings();
            return setting;
        }

        /// <summary>
        /// Enum which represents a type of provider.
        /// </summary>
        public enum SettingType
        {
            /// <summary>
            /// The provider is custom, added by a third party package. as a result, the provider is not created by R2EK
            /// </summary>
            Custom = -1,

            /// <summary>
            /// The ProjectSettings EditorSettings, this is a default provider from R2EK
            /// </summary>
            ProjectSetting = 0,

            /// <summary>
            /// The UserSettings EditorSettings, this is a default provider from R2EK
            /// </summary>
            UserSetting = 1,
        }

        /// <summary>
        /// Interface that marks a Class as a SettingProvider. recommended to be implemented to <see cref="ScriptableSingleton{T}"/>s
        /// </summary>
        public interface IEditorSettingProvider
        {
            /// <summary>
            /// The EditorSettings stored within the provider
            /// </summary>
            List<EditorSettingCollection> editorSettings { get; }

            /// <summary>
            /// The name for the provider.
            /// </summary>
            string providerName { get; }

            /// <summary>
            /// Saves the settings for this provider.
            /// </summary>
            void SaveSettings();
        }
    }
}