using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="R2EKEditorProjectSettings"/> is a <see cref="ScriptableSingleton{T}"/> that implements <see cref="EditorSettingManager.IEditorSettingProvider"/>. It stores <see cref="EditorSettingCollection"/>s that are used for this specific project.
    /// 
    /// <para>Utilizing any of the methods within <see cref="EditorSettingManager"/> that accepts <see cref="EditorSettingManager.SettingType"/>, and using the <see cref="EditorSettingManager.SettingType.ProjectSetting"/> uses this provider.</para>
    /// <para>For user specific related settings, see <see cref="R2EKEditorPreferenceSettings"/></para>
    /// </summary>
    [FilePath("ProjectSettings/RoR2EditorKit/R2EKEditorProjectSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class R2EKEditorProjectSettings : ScriptableSingleton<R2EKEditorProjectSettings>, EditorSettingManager.IEditorSettingProvider
    {
        [SerializeField]
        private List<EditorSettingCollection> _projectSettings = new List<EditorSettingCollection>();

        List<EditorSettingCollection> EditorSettingManager.IEditorSettingProvider.editorSettings => _projectSettings;
        string EditorSettingManager.IEditorSettingProvider.providerName => nameof(R2EKEditorProjectSettings);
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

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorSettingManager.RegisterProvider(instance, () => instance);
        }

        /// <summary>
        /// Opens the ProjectSettings window and selects these settings.
        /// </summary>
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Settings/Project Editor Settings")]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/R2EK Editor Settings");
    }
}
