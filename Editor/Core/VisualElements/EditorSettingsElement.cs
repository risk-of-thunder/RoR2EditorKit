using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    internal class EditorSettingsElement : VisualElement
    {
        public EditorSettingManager.IEditorSettingProvider provider { get; }
        public EditorSetting currentlyInspectedSetting
        {
            get => _currentlyInspectedSetting;
            set
            {
                if(_currentlyInspectedSetting != value)
                {
                    _currentlyInspectedSetting = value;
                    OnCurrentlyInspectedSettingChange();
                }
            }
        }
        private EditorSetting _currentlyInspectedSetting;
        public ListView editorSettingSelectionListView { get; }
        public ListView editorSettingValueSelectionListView { get; }
        private VisualElement _editorSettingValueSelectionContainer;
        private Label _editorSettingValueSelectionLabel;
        public Button saveSettingsButton { get; }
        private void OnAttached(AttachToPanelEvent evt)
        {
            editorSettingSelectionListView.itemsSource = provider.editorSettings;
            editorSettingSelectionListView.makeItem = CreateEditorSettingButton;
            editorSettingSelectionListView.bindItem = BindEditorSettingButton;

            OnCurrentlyInspectedSettingChange();
            editorSettingValueSelectionListView.makeItem = CreateEditorSettingValueContainer;
            editorSettingValueSelectionListView.bindItem = CreateEditorSettingValueControl;
        }

        private void OnCurrentlyInspectedSettingChange()
        {

            editorSettingValueSelectionListView.itemsSource = currentlyInspectedSetting?._serializedSettings ?? null;
            _editorSettingValueSelectionContainer.SetDisplay(currentlyInspectedSetting != null);
            _editorSettingValueSelectionLabel.text = currentlyInspectedSetting != null ? $"{ObjectNames.NicifyVariableName(currentlyInspectedSetting._typeName)}'s Settings" : string.Empty;
        }

        private void BindEditorSettingButton(VisualElement element, int arg2)
        {
            var button = (Button)element;
            var setting = provider.editorSettings[arg2];
            button.clicked += () =>
            {
                currentlyInspectedSetting = setting;
            };
            button.text = ObjectNames.NicifyVariableName(setting._typeName);
            button.tooltip = setting._editorTypeQualifiedName;
        }

        private VisualElement CreateEditorSettingButton()
        {
            return new Button();
        }

        private void CreateEditorSettingValueControl(VisualElement element, int arg2)
        {
            var settings = currentlyInspectedSetting._serializedSettings;
            var settingValue = settings[arg2];

            var settingType = settingValue.settingType;
            var label = ObjectNames.NicifyVariableName(settingValue._settingName);
            Func<object> getter = () => settingValue.boxedValue;
            VisualElementUtil.DeconstructedChangeEvent changeEvent = (evt) =>
            {
                settingValue.boxedValue = evt.newValue;
                settings[arg2] = settingValue;
            };
            element.Add(VisualElementUtil.CreateControlFromType(settingType, label, getter, changeEvent));
        }

        private VisualElement CreateEditorSettingValueContainer()
        {
            return new VisualElement();
        }

        private void OnDetached(DetachFromPanelEvent evt)
        {
            editorSettingSelectionListView.bindItem = null;
            editorSettingSelectionListView.makeItem = null;
            editorSettingSelectionListView.itemsSource = null;

            editorSettingValueSelectionListView.makeItem = null;
            editorSettingValueSelectionListView.binding = null;
        }

        public EditorSettingsElement(EditorSettingManager.IEditorSettingProvider provider)
        {
            this.provider = provider;
            VisualElementTemplateDictionary.instance.GetTemplateInstance(nameof(EditorSettingsElement), this);

            editorSettingSelectionListView = this.Q<ListView>("EditorSettingSelectionListView");
            editorSettingValueSelectionListView = this.Q<ListView>("EditorSettingValueSelectionListView");
            _editorSettingValueSelectionContainer = editorSettingValueSelectionListView.parent;
            _editorSettingValueSelectionLabel = _editorSettingValueSelectionContainer.Q<Label>();
            saveSettingsButton = this.Q<Button>("SaveSettings");

            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        ~EditorSettingsElement()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttached);
            UnregisterCallback<DetachFromPanelEvent>(OnDetached);
        }
    }
}