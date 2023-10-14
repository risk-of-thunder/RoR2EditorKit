using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoR2EditorKit.Data
{
    public class EditorSetting
    {
        public Type ownerType;
        private ReadOnlyDictionary<int, object> settingIDToValue;

        public T GetSettingValue<T>(string settingID, T defaultValue = default)
        {
            return GetSettingValue<T>(settingID.GetHashCode());
        }

        public T GetSettingValue<T>(int settingID, T defaultValue = default)
        {
            if(settingIDToValue.TryGetValue(settingID, out var value))
            {
                return (T)value;
            }
            SetSettingValue(settingID, defaultValue);
            return defaultValue;
        }

        public void SetSettingValue(string settingID, object value)
        {
            SetSettingValue(settingID.GetHashCode(), value);
        }

        public void SetSettingValue(int settingID, object value)
        {
            var settings = EditorSettings.GetOrCreateSettings<EditorSettings>();
            settings.SetSettingValue(value, ownerType, settingID);
        }

        
        internal EditorSetting(Dictionary<int, object> dictionary)
        {
            settingIDToValue = new ReadOnlyDictionary<int, object>(dictionary);
        }
    }
}
