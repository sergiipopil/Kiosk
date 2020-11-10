using System.IO;

namespace KioskBrains.Kiosk.CommonNet.Helpers
{
    public static class FolderHelper
    {
        // Regular issue for .git folders
        public static void DeleteFolderWithReadonlyFiles(string folderPath)
        {
            var directory = new DirectoryInfo(folderPath)
                {
                    Attributes = FileAttributes.Normal
                };
            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }
            directory.Delete(true);
        }

        public static void CopyFolderWithReplace(string sourceFolderPath, string targetFolderPath)
        {
            // create target folder
            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            // create target subfolders
            foreach (var folderPath in Directory.GetDirectories(sourceFolderPath, "*", SearchOption.AllDirectories))
            {
                var newTargetFolderPath = folderPath.Replace(sourceFolderPath, targetFolderPath);
                if (!Directory.Exists(newTargetFolderPath))
                {
                    Directory.CreateDirectory(newTargetFolderPath);
                }
            }

            // copy/replace all files
            foreach (var filePath in Directory.GetFiles(sourceFolderPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(sourceFolderPath, targetFolderPath), true);
            }
        }
    }
}