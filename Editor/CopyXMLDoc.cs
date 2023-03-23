using System.IO;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit
{
    [InitializeOnLoad]
    internal static class CopyXMLDoc
    {
        static CopyXMLDoc()
        {
            if (ShouldCopy())
            {
                Debug.Log($"Copying over the ROR2EK XML Doc");
                DoCopy();
            }
        }

        private static bool ShouldCopy()
        {
            var relativePath = Constants.AssetGUIDS.GetPath(Constants.AssetGUIDS.xmlDocGUID);
            var fullPath = Path.GetFullPath(relativePath);
            var fileName = Path.GetFileName(fullPath);
            var pathToCheck = Path.Combine(Constants.FolderPaths.ScriptAssembliesFolder, fileName);
            return !File.Exists(pathToCheck);
        }

        private static void DoCopy()
        {
            var relativePath = Constants.AssetGUIDS.GetPath(Constants.AssetGUIDS.xmlDocGUID);
            var sourcePath = Path.GetFullPath(relativePath);
            var fileName = Path.GetFileName(sourcePath);
            var destPath = Path.Combine(Constants.FolderPaths.ScriptAssembliesFolder, fileName);
            File.Copy(sourcePath, destPath, true);
        }
    }
}
