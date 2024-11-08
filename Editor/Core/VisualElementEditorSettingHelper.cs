using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="VisualElementEditorSettingHelper"/> is a class that allows you to connect a <see cref="INotifyValueChanged{T}"/> element to an EditorSetting
    /// </summary>
    public static class VisualElementEditorSettingHelper
    {
        private static FixedConditionalWeakTable<VisualElement, SettingData> _elementToData = new FixedConditionalWeakTable<VisualElement, SettingData>();

        /// <summary>
        /// Connects the <see cref="INotifyValueChanged{T}"/> implementing element to the selected setting.
        /// </summary>
        /// <typeparam name="T">The type of value being stored</typeparam>
        /// <param name="notifyValueChanged">The VisualElement that updates the editor setting</param>
        /// <param name="setting">The collection which will store the setting</param>
        /// <param name="settingName">The setting's name</param>
        /// <param name="defaultValue">a defualt value, in case the setting does not exist in the collection</param>
        public static void ConnectWithSetting<T>(this INotifyValueChanged<T> notifyValueChanged, EditorSettingCollection setting, string settingName, T defaultValue = default(T))
        {
            if (!EditorStringSerializer.CanSerializeType<T>())
            {
                Debug.LogError($"Cannot connect element {notifyValueChanged} to editor settings, requested type {typeof(T).Name} is not supported.");
                return;
            }

            PrepareElement((VisualElement)notifyValueChanged, setting, settingName, defaultValue);

            notifyValueChanged.value = setting.GetOrCreateSetting(settingName, defaultValue);
            notifyValueChanged.RegisterValueChangedCallback(OnValueChanged);
        }

        private static void OnValueChanged<T>(ChangeEvent<T> evt)
        {
            VisualElement target = (VisualElement)evt.target;
            if (_elementToData.TryGetValue(target, out var data))
            {
                data.setting.SetSettingValue(data.settingName, evt.newValue);
            }
        }

        private static void PrepareElement<T>(VisualElement element, EditorSettingCollection setting, string settingName, T defaultValue)
        {
            var data = _elementToData.GetValue(element, x => new SettingData());
            data.setting = setting;
            data.settingName = settingName;
            data.defaultValue = defaultValue;
        }

        private class SettingData
        {
            public EditorSettingCollection setting;
            public string settingName;
            public object defaultValue;
        }
    }
}