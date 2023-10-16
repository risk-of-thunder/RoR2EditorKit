using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Data
{
    internal class EditorSettingDrawer : VisualElement
    {
        public EditorSetting CurrentSetting { get; set; }

        private IMGUIContainer _imguiContainer;

        public void DrawSettings()
        {
            if (CurrentSetting == null)
                return;

            foreach(var setting in CurrentSetting._serializedSettings)
            {
                var settingType = Type.GetType(setting.valueTypeQualifiedName);
                if(!IMGUIUtil.CanDrawFieldFromType(settingType))
                {
                    EditorGUILayout.LabelField($"Cannot draw setting of name {setting.settingName}. (SettingValueType={settingType.FullName}");
                    continue;
                }

                var deserialized = EditorStringSerializer.Deserialize(setting.value, settingType);
                GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(setting.settingName));
                if(IMGUIUtil.DrawFieldWithType(settingType, deserialized, out object newValue, label))
                {
                    setting.value = EditorStringSerializer.Serialize(newValue, settingType);
                }
            }
        }
        public EditorSettingDrawer()
        {
            _imguiContainer = new IMGUIContainer();
            _imguiContainer.onGUIHandler = DrawSettings;
            Add(_imguiContainer);
        }
    }
}
