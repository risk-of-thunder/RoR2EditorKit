using System.IO;
using UnityEditor;

namespace RoR2.Editor
{
    /// <summary>
    /// Helper class that defines a "Per User Project Setting" directory.
    /// </summary>
    public static class PerUserProjectSettingHelper
    {
        public const string ROOT_FILE_PATH = "ProjectSettings/RoR2EditorKit/PerUser/";

        [InitializeOnLoadMethod]
        private static void EnsureGitIgnore()
        {
            if(!Directory.Exists(R2EKConstants.FolderPaths.r2EKPerUserProjectSettingsPath))
            {
                Directory.CreateDirectory(R2EKConstants.FolderPaths.r2EKPerUserProjectSettingsPath);
            }

            var gitignorePath = Path.Combine(R2EKConstants.FolderPaths.r2EKPerUserProjectSettingsPath, ".gitignore");

            if (File.Exists(gitignorePath))
                return;

            string gitignoreContents = "*.*";
            using(var writer = File.CreateText(gitignorePath))
            {
                writer.Write(gitignoreContents);
            }
        }
    }
}