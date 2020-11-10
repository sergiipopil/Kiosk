using System.Collections.Generic;

namespace KioskBrains.Kiosk.Core.Devices.Common
{
    public delegate bool ResponseReadingFinalizer(IList<byte> currentMessageBytes);
}