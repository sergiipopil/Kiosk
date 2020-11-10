using System.Collections.Generic;

namespace KioskBrains.Kiosk.Core.Devices.Documents
{
    public class DocumentsStateModel
    {
        public Dictionary<string, int> CurrentNumbers { get; set; }

        public bool IsValid()
        {
            return CurrentNumbers != null;
        }
    }
}