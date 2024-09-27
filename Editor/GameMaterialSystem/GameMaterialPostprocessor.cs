using UnityEngine;
using UnityEditor;
using RoR2.Editor.GameMaterialSystem;
using System.Collections.Generic;
using RoR2.Editor;
using System;

class GameMaterialPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        if (didDomainReload)
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                var allAssets = SerializableMaterialDataImporter.instances;
                for(int i = allAssets.Count - 1; i >= 0; i--)
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(allAssets[i]));
                }
            }
            catch(Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }
    }
}