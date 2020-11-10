using System;

namespace KioskBrains.Server.Domain.Helpers.Files
{
    public static class FileHelper
    {
        public static Guid GenerateNewFileKey()
        {
            return Guid.NewGuid();
        }

        public static string FileKeyToString(Guid fileKey)
        {
            return fileKey.ToString("N");
        }

        public static Guid ParseFileKey(string fileKeyString)
        {
            return Guid.Parse(fileKeyString);
        }
    }
}