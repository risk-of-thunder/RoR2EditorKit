using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Data
{
    internal class EditorSettingsButton : Button
    {
        public int SettingIndex => _settingIndex;
        private int _settingIndex;

        public EditorSetting TiedSetting => _editorSetting;
        public EditorSetting _editorSetting;

        public void SetSetting(EditorSetting setting, int index)
        {
            _settingIndex = index;
            _editorSetting = setting;

            text = ObjectNames.NicifyVariableName(_editorSetting._typeName);
        }

        private void ResetAllSettings(DropdownMenuAction action)
        {
            foreach(var settingName in TiedSetting.AllSettingNames)
            {
                _editorSetting.ResetSetting(settingName);
            }
        }

        public EditorSettingsButton()
        {
            this.AddSimpleContextMenu(new ContextMenuData
            {
                menuName = "Reset All Settings",
                menuAction = ResetAllSettings,
                actionStatusCheck = (_) => DropdownMenuAction.Status.Normal
            });
        }
    }
}