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
    public sealed class R2EditorSettings : ThunderKitSetting
    {
        public bool enableNamingConventions = true;
        [ReadOnly, SerializeField]
        private List<EditorSetting> _settings = new List<EditorSetting>();

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

        public T GetSetting<T>(Type editorType, string settingName, T defaultValue = default)
        {
            EditorSetting setting = GetOrCreateSettingsFor(editorType);
            return setting.GetSetting<T>(settingName, defaultValue);
        }

        public void SetSetting(object value, Type type, string settingName)
        {
            EditorSetting setting = GetOrCreateSettingsFor(type);
            setting.SetSetting(settingName, value);
        }

        public void ResetSettingValue(Type type, string settingName)
        {
            EditorSetting setting = GetOrCreateSettingsFor(type);
            setting.ResetSetting(settingName);
        }

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