using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2EditorKit.Data
{
    [Serializable]
    public struct SerializedEditorSettings
    {
        public string editorTypeQualifiedName;
        public List<SerializedSetting> serializedSettings;

        public void SetSetting(int hashSetting, object value)
        {
            for (int i = 0; i < serializedSettings.Count; i++)
            {
                var setting = serializedSettings[i];

                if (setting.valueID == hashSetting)
                {
                    setting.valueTypeQualifiedName = value.GetType().AssemblyQualifiedName;
                    setting.value = EditorStringSerializer.Serialize(value, value.GetType());
                    serializedSettings[i] = setting;
                    return;
                }
            }
            serializedSettings.Add(new SerializedSetting
            {
                valueTypeQualifiedName = value.GetType().AssemblyQualifiedName,
                value = EditorStringSerializer.Serialize(value, value.GetType()),
                valueID = hashSetting,
            });
        }

        public static implicit operator EditorSetting(SerializedEditorSettings serialized)
        {
            var ownerType = Type.GetType(serialized.editorTypeQualifiedName);

            Dictionary<int, object> settingIDToValue = new Dictionary<int, object>();
            foreach(var serializedSetting in serialized.serializedSettings)
            {
                var settingType = Type.GetType(serializedSetting.valueTypeQualifiedName);
                settingIDToValue.Add(serializedSetting.valueID, EditorStringSerializer.Deserialize(serializedSetting.value, settingType));
            }

            return new EditorSetting(settingIDToValue)
            {
                ownerType = ownerType
            };
        }

        [Serializable]
        public struct SerializedSetting
        {
            public int valueID;

            public string valueTypeQualifiedName;
            public string value;
        }
    }
}
