using System.IO;
using UnityEditor;

namespace RoR2.Editor
{
    public static class IOUtils
    {
        public static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        public static string FormatPathForUnity(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string GetCurrentDirectory()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        public static string GenerateUniqueFileName(string folderPath, string baseFileName, string extension)
        {
            if (!folderPath.EndsWith("/"))
            {
                folderPath += "/";
            }

            string fileName = baseFileName;
            string filePath = folderPath + fileName + extension;

            int counter = 1;
            while (File.Exists(filePath))
            {
                fileName = baseFileName + "_" + counter;
                filePath = folderPath + fileName + extension;
                counter++;
            }

            return folderPath + fileName + extension;
        }

        public static string GetUniquePrefabPath(string folderPath, string baseFileName)
        {
            string uniqueFileName = GenerateUniqueFileName(folderPath, baseFileName, ".prefab");
            return folderPath + uniqueFileName;
        }
    }
}