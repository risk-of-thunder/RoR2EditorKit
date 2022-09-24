using System.IO;
using UnityEditor;

namespace RoR2EditorKit.Utilities
{
    /// <summary>
    /// General System.IO related utilities.
    /// </summary>
    public static class IOUtils
    {
        /// <summary>
        /// If the directory specified in <paramref name="directoryPath"/> does not exist, it creates it.
        /// </summary>
        /// <param name="directoryPath">The directory path to ensure its existence</param>
        public static void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }

        /// <summary>
        /// Formats <paramref name="path"/> so its valid for unity's systems.
        /// Replaces \ with /
        /// </summary>
        /// <param name="path">The path to modified</param>
        /// <returns>The formatted path</returns>
        public static string FormatPathForUnity(string path)
        {
            return path.Replace("\\", "/");
        }

        /// <summary>
        /// Returns the current directory from the active object.
        /// </summary>
        /// <returns>The active object'sdirectory</returns>
        public static string GetCurrentDirectory()
        {
            var activeObjectPath = Path.GetFullPath(AssetDatabase.GetAssetPath(Selection.activeObject));
            if (File.Exists(activeObjectPath))
            {
                activeObjectPath = Path.GetDirectoryName(activeObjectPath);
            }
            return activeObjectPath;
        }
    }
}
