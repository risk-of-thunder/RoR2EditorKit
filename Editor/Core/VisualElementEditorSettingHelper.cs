using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public static class VisualElementEditorSettingHelper
    {
        private static FixedConditionalWeakTable<VisualElement, SettingData> _elementToData = new FixedConditionalWeakTable<VisualElement, SettingData>();
        public static void ConnectWithSetting<T>(this INotifyValueChanged<T> notifyValueChanged, EditorSetting setting, string settingName, T defaultValue = default(T))
        {
            if(!EditorStringSerializer.CanSerializeType<T>())
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
            if(_elementToData.TryGetValue(target, out var data))
            {
                data.setting.SetSettingValue(data.settingName, evt.newValue);
            }
        }

        private static void PrepareElement<T>(VisualElement element, EditorSetting setting, string settingName, T defaultValue)
        {
            var data = _elementToData.GetValue(element, x => new SettingData());
            data.setting = setting;
            data.settingName = settingName;
            data.defaultValue = defaultValue;
        }

        private class SettingData
        {
            public EditorSetting setting;
            public string settingName;
            public object defaultValue;
        }
    }
}