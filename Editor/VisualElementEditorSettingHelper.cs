using RoR2EditorKit;
using RoR2EditorKit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit
{
    public static class VisualElementEditorSettingHelper
    {
        private static FixedConditionalWeakTable<VisualElement, SettingData> elementToData = new FixedConditionalWeakTable<VisualElement, SettingData>();
        public static void ConnectWithSetting<T>(this INotifyValueChanged<T> notifyValueChanged, EditorSetting setting, string settingName, T defaultValue = default)
        {
            PrepareElement((VisualElement)notifyValueChanged, setting, settingName, defaultValue);

            notifyValueChanged.value = setting.GetSetting(settingName, defaultValue);
            notifyValueChanged.RegisterValueChangedCallback(OnValueChanged);
        }

        private static void OnValueChanged<T>(ChangeEvent<T> evt)
        {
            VisualElement target = (VisualElement)evt.target;
            var settingsData = elementToData.GetValue(target, null);
            settingsData.setting.SetSetting(settingsData.settingName, evt.newValue);
        }

        private static void PrepareElement(VisualElement element, EditorSetting setting, string settingName, object defaultValue)
        {
            var data = elementToData.GetValue(element, x => new SettingData());
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
