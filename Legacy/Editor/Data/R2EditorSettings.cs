using System.Collections.Generic;
using UnityEditor;
using ThunderKit.Core.Data;
using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UIElements;
using ThunderKit.Core.UIElements;
using RoR2EditorKit.VisualElements;
using System.Globalization;
using UnityEditor.UIElements;

namespace RoR2EditorKit.Data
{
    /// <summary>
    /// The R2EditorSettings is a ThunderKitSetting that replaces the now defunct InspectorSettings file.
    /// <br>The class has the capabilities of creating unique, serialized settings for any kind of class, these settings can then be deserialized into valid values to be used in scripts, or serialized for future usage.</br>
    /// </summary>
    public sealed class R2EditorSettings : ThunderKitSetting
    {
        /// <summary>
        /// Wether the naming convention system of R2EK is enabled
        /// </summary>
        public bool enableNamingConventions = true;
        [ReadOnly, SerializeField]
        private List<EditorSetting> _settings = new List<EditorSetting>();

        /// <summary>
        /// Access to RoR2EditorKit's main settings file
        /// </summary>
        public RoR2EditorKitSettings MainSettings => GetOrCreateSettings<RoR2EditorKitSettings>();

        private SerializedObject _editorSettingsSO;
        private ExtendedListView _settingsButtons;
        private Label _settingsViewText;
        private EditorSettingDrawer _drawer;
        private SerializedProperty _settingsProperty;

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            _editorSettingsSO = new SerializedObject(this);
            _settingsProperty = _editorSettingsSO.FindProperty("_settings");
            TemplateHelpers.GetTemplateInstance("EditorSettingsWindow", rootElement, VisualElementUtil.ValidateUXMLPath);

            _settingsViewText = rootElement.Q<Label>("SubtitleText");

            _drawer = new EditorSettingDrawer();
            var _settingsView = rootElement.Q<ScrollView>("EditorSettingView");
            _settingsView.Add(_drawer);

            _settingsButtons = rootElement.Q<ExtendedListView>("SettingsButtons");
            _settingsButtons.CreateElement = CreateSettingsButton;
            _settingsButtons.BindElement = BindSettingsButton;
            _settingsButtons.collectionProperty = _settingsProperty;

            rootElement.Bind(_editorSettingsSO);
        }

        private Button CreateSettingsButton()
        {
            return new EditorSettingsButton();
        }

        private void BindSettingsButton(VisualElement element, SerializedProperty property)
        {
            int index = int.Parse(element.name.Substring("Element".Length), CultureInfo.InvariantCulture);
            EditorSetting tiedSetting = _settings[index];
            EditorSettingsButton button = (EditorSettingsButton)element;
            button.style.height = _settingsButtons.listViewItemHeight;
            button.SetSetting(tiedSetting, index);
            button.clickable.clickedWithEventInfo += SetSettingsToView;
        }

        private void SetSettingsToView(EventBase @base)
        {
            EditorSettingsButton settingsButton = (EditorSettingsButton)@base.target;
            EditorSetting setting = settingsButton.TiedSetting;
            _settingsViewText.text = ObjectNames.NicifyVariableName(setting._typeName);

            _drawer.CurrentSetting = setting;
        }

        /// <summary>
        /// Retrieves the setting of name <paramref name="settingName"/> stored in the EditorSetting for type <paramref name="editorType"/>.
        /// <br>* If no EditorSetting exists for the type <paramref name="editorType"/>, a new one is created.</br>
        /// <br>* If no Setting of name <paramref name="settingName"/> exists in the EditorSetting, a new one is created using the default value specified in <paramref name="defaultValue"/></br>
        /// </summary>
        /// <typeparam name="T">The type of the setting</typeparam>
        /// <param name="editorType">The Type that's storing the value</param>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value to use in case no setting exists.</param>
        /// <returns>The setting's value</returns>
        public T GetSetting<T>(Type editorType, string settingName, T defaultValue = default)
        {
            EditorSetting setting = GetOrCreateSettingsFor(editorType);
            return setting.GetSetting<T>(settingName, defaultValue);
        }

        /// <summary>
        /// Sets the value of the setting named <paramref name="settingName"/> stored in the EditorSetting for type <paramref name="editorType"/>.
        /// <br>* If no EditorSetting exists for the type <paramref name="editorType"/>, a new one is created.</br>
        /// <br>* If no Setting of name <paramref name="settingName"/> exists in the EditorSetting, a new one is created using the default value specified in <paramref name="defaultValue"/></br>
        /// <br>* An <see cref="InvalidOperationException"/> gets thrown if a setting of name <paramref name="settingName"/> exists, but the ValueType of the setting is not the type of <paramref name="value"/></br>
        /// <para>for a valid list of serializable values, see <see cref="EditorStringSerializer"/></para>
        /// </summary>
        /// <param name="editorType">The Type that's storing the value</param>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="value">The value to set</param>
        public void SetSetting(Type editorType, string settingName, object value)
        {
            EditorSetting setting = GetOrCreateSettingsFor(editorType);
            setting.SetSetting(settingName, value);
        }

        /// <summary>
        /// Resets the value of the setting named <paramref name="settingName"/> to it's default value
        /// <br>* If no EditorSetting exists for the type <paramref name="editorType"/>, a new one is created.</br>
        /// <br>* If no setting of name <paramref name="settingName"/> exists, no setting is reset.</br>
        /// </summary>
        /// <param name="settingName">The setting to reset</param>
        public void ResetSettingValue(Type type, string settingName)
        {
            EditorSetting setting = GetOrCreateSettingsFor(type);
            setting.ResetSetting(settingName);
        }

        /// <summary>
        /// Gets or Creates the EditorSetting for the type specified in <paramref name="type"/>
        /// </summary>
        /// <param name="type">The type of EditorSetting to retrieve or create</param>
        /// <returns>The EditorSetting for the type.</returns>
        public EditorSetting GetOrCreateSettingsFor(Type type)
        {
            int hash = type.AssemblyQualifiedName.GetHashCode();
            foreach(var setting in _settings)
            {
                if(hash == setting._editorTypeQualifiedHash)
                {
                    return setting;
                }
            }
            return CreateSettingsFor(type);
        }

        private EditorSetting CreateSettingsFor(Type editorType)
        {
            var setting = new EditorSetting
            {
                _typeName = editorType.Name,
                _editorTypeQualifiedName = editorType.AssemblyQualifiedName,
                _editorTypeQualifiedHash = editorType.AssemblyQualifiedName.GetHashCode(),
                _serializedSettings = new List<EditorSetting.SettingValue>()
            };
            _settings.Add(setting);
            _settings.Sort((x, y) =>
            {
                return x._typeName.CompareTo(y._typeName);
            });
            return setting;
        }
    }
}