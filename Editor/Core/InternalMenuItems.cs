using UnityEditor;

namespace RoR2.Editor
{
    internal class InternalMenuItems
    {
        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Utility/Reload Domain")]
        private static void ReloadDomain()
        {
            EditorUtility.RequestScriptReload();
        }

        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Utility/Save Assets")]
        private static void SaveAssets()
        {
            AssetDatabase.SaveAssets();
        }

        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Utility/Refresh Asset Database")]
        private static void RefreshDatabase()
        {
            AssetDatabase.Refresh();
        }
    }
}