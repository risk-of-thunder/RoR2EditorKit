using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// An <see cref="EditorSettingsElement"/> is a VisualElement that exposes the settings stored by a <see cref="EditorSettingManager.IEditorSettingProvider"/> in a way that can be easily modified by the end user.
    /// </summary>
    public class EditorSettingsElement : VisualElement
    {
        /// <summary>
        /// The provider from which we're modifying it's settings
        /// </summary>
        public EditorSettingManager.IEditorSettingProvider provider { get; }

        /// <summary>
        /// The currently inspected EditorSetting, changing this value changes the settings being inspected
        /// </summary>
        public EditorSettingCollection currentlyInspectedSetting
        {
            get => _currentlyInspectedSetting;
            set
            {
                if (_currentlyInspectedSetting != value)
                {
                    _currentlyInspectedSetting = value;
                    OnCurrentlyInspectedSettingChange();
                }
            }
        }
        private EditorSettingCollection _currentlyInspectedSetting;

        /// <summary>
        /// The list view that contains and displays all the <see cref="EditorSettingCollection"/> stored within <see cref="provider"/>
        /// </summary>
        public ListView editorSettingSelectionListView { get; }

        /// <summary>
        /// The List view that contains the controls to modify the settings stored within <see cref="currentlyInspectedSetting"/>
        /// </summary>
        public ListView editorSettingValueSelectionListView { get; }

        private VisualElement _editorSettingValueSelectionContainer;
        private Label _editorSettingValueSelectionLabel;

        /// <summary>
        /// The button which on click will save the settings to disk
        /// </summary>
        public Button saveSettingsButton { get; }

        private List<EditorSettingCollection> _settingsThatAreNotOrphaned = new List<EditorSettingCollection>();

        private void OnAttached(AttachToPanelEvent evt)
        {
            _settingsThatAreNotOrphaned.Clear();
            _settingsThatAreNotOrphaned.AddRange(provider.editorSettings.Where(FilterOrphans));
            editorSettingSelectionListView.itemsSource = _settingsThatAreNotOrphaned;
            editorSettingSelectionListView.makeItem = CreateEditorSettingButton;
            editorSettingSelectionListView.bindItem = BindEditorSettingButton;

            OnCurrentlyInspectedSettingChange();
            editorSettingValueSelectionListView.makeItem = CreateEditorSettingValueContainer;
            editorSettingValueSelectionListView.bindItem = CreateEditorSettingValueControl;
        }

        private bool FilterOrphans(EditorSettingCollection setting)
        {
            return setting.ownerType != null;
        }

        private void OnCurrentlyInspectedSettingChange()
        {

            editorSettingValueSelectionListView.itemsSource = currentlyInspectedSetting?._serializedSettings ?? null;
            _editorSettingValueSelectionContainer.SetDisplay(currentlyInspectedSetting != null);
            _editorSettingValueSelectionLabel.text = currentlyInspectedSetting != null ? $"{ObjectNames.NicifyVariableName(currentlyInspectedSetting._typeName)}'s Settings" : string.Empty;
            editorSettingValueSelectionListView.Rebuild();
        }

        private void BindEditorSettingButton(VisualElement element, int arg2)
        {
            var button = (Button)element;
            var setting = _settingsThatAreNotOrphaned[arg2];
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

        /// <summary>
        /// Creates a new instance of <see cref="EditorSettingsElement"/>
        /// </summary>
        /// <param name="provider">The provider from where we'll get the EditorSettings</param>
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