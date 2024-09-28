using System.IO;
using UnityEditor;

namespace RoR2.Editor
{
    /// <summary>
    /// Contains methods that interacts with IO Paths
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        /// Ensures the creation of a directory
        /// </summary>
        /// <param name="directoryPath">The directory to ensure</param>
        public static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        /// <summary>
        /// Formats a path for unity, essentially replacing backslashes with forward slashes
        /// </summary>
        /// <param name="path">The path to format</param>
        /// <returns>The formatted path</returns>
        public static string FormatPathForUnity(string path)
        {
            return path.Replace("\\", "/");
        }

        /// <summary>
        /// Returns the current directory selected
        /// </summary>
        /// <returns>The current selected directory, if no directory is selected, "Assets" is returned instead</returns>
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

        /// <summary>
        /// Generates a unique file name and path for an asset
        /// </summary>
        /// <param name="folderPath">The folder path which will hold our asset</param>
        /// <param name="baseFileName">The base name for the asset</param>
        /// <param name="extension">The asset's extension</param>
        /// <returns>A unique file name and path for an asset</returns>
        public static string GenerateUniqueFileName(string folderPath, string baseFileName, string extension)
        {
            if (!folderPath.EndsWith("/"))
            {
                folderPath += "/";
            }
            extension = extension.StartsWith('.') ? extension : ('.' + extension);

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

        /// <summary>
        /// Generates a unique file name and path for a prefab
        /// </summary>
        /// <param name="folderPath">The folder path which will hold our prefab</param>
        /// <param name="baseFileName">The base name for this prefab, must not contain the ".prefab" extension</param>
        /// <returns>A unique file name and path for the prefab</returns>
        public static string GetUniquePrefabPath(string folderPath, string baseFileName)
        {
            string uniqueFileName = GenerateUniqueFileName(folderPath, baseFileName, ".prefab");
            return folderPath + uniqueFileName;
        }
    }
}