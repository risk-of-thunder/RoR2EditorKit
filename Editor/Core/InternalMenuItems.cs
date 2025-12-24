using System;
using System.IO;
using UnityEditor;
using UnityEngine;

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

        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Re-Import Addressable Catalog")]
        private static void ReimportAddressableCatalog()
        {
            R2EKPreferences instance = R2EKPreferences.instance;
            if (!File.Exists(instance.addressableAssetsCatalog)) return;
            if (!File.Exists(instance.addressableAssetsSettings)) return;

            try
            {
                string destinationFolder = Path.Combine("Assets", "StreamingAssets", "aa");
                Directory.CreateDirectory(destinationFolder);

                var destinationCatalog = Path.Combine(destinationFolder, "catalog.json");
                var destinationSettings = Path.Combine(destinationFolder, "settings.json");
                if (File.Exists(destinationCatalog)) File.Delete(destinationCatalog);
                if (File.Exists(destinationSettings)) File.Delete(destinationSettings);

                File.Copy(instance.addressableAssetsCatalog, destinationCatalog);
                File.Copy(instance.addressableAssetsSettings, destinationSettings);
            }
            catch (Exception e)
            {
                RoR2EKLog.Error(e);
            }
        }
    }
}