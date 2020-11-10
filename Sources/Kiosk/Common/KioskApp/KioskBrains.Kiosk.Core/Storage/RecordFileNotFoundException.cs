using System;

namespace KioskBrains.Kiosk.Core.Storage
{
    public class RecordFileNotFoundException : Exception
    {
        public RecordFileNotFoundException(string folderName, string fileName)
            : base($"'{folderName}\\{fileName}' was not found.")
        {
        }
    }
}
