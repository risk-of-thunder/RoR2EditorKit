using UnityEngine;
using UnityEditor;
using RoR2.Editor.GameMaterialSystem;
using System.Collections.Generic;

class MyAllPostprocessor : AssetPostprocessor
{
    static List<Shader> realShaders = new List<Shader>();
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        if (didDomainReload)
        {
            foreach(var stubbedShader in GameMaterialSystemSettings.instance.stubbedShaders)
            {
                if(GameMaterialSystemSettings.instance.TryLoadAddressableShader(stubbedShader.shader, out var real))
                {
                    realShaders.Add(real);
                }
            }
        }
    }
}