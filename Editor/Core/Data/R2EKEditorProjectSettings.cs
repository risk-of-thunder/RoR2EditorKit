using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    [FilePath("ProjectSettings/RoR2EditorKit/R2EKEditorProjectSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class R2EKEditorProjectSettings : ScriptableSingleton<R2EKEditorProjectSettings>, EditorSettingManager.IEditorSettingProvider
    {
        [SerializeField]
        private List<EditorSetting> _projectSettings = new List<EditorSetting>();

        List<EditorSetting> EditorSettingManager.IEditorSettingProvider.editorSettings => _projectSettings;
        public void SaveSettings() => Save(true);

        private void Awake()
        {
            EditorApplication.quitting += EditorApplication_quitting;
        }

        private void EditorApplication_quitting()
        {
            Save(true);
        }

        private void OnDestroy()
        {
            EditorApplication.quitting -= EditorApplication_quitting;
        }


        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Project Editor Settings")]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/R2EK Editor Settings");
    }
}
