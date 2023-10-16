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
    /// <summary>
    /// Class containing utility methods for binding VisualElements to settings stored in <see cref="EditorSetting"/>
    /// </summary>
    public static class VisualElementEditorSettingHelper
    {
        private static FixedConditionalWeakTable<VisualElement, SettingData> elementToData = new FixedConditionalWeakTable<VisualElement, SettingData>();

        /// <summary>
        /// Connects a VisualElement that implements <see cref="INotifyValueChanged{T}"/> to a <paramref name="settingName"/> inside <paramref name="setting"/>.
        /// <br>If no setting of name <paramref name="settingName"/> exists in <paramref name="setting"/>, a new one is created using the value specified in <paramref name="defaultValue"/></br>
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="notifyValueChanged">The element thats being bound</param>
        /// <param name="setting">The EditorSeting where the setting specified in <paramref name="settingName"/> is stored</param>
        /// <param name="settingName">The Setting's name</param>
        /// <param name="defaultValue">A default value to use if no setting exists.</param>
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
