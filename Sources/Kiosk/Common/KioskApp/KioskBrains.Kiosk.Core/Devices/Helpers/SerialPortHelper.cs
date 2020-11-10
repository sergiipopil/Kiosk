using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;

namespace KioskBrains.Kiosk.Core.Devices.Helpers
{
    public static class SerialPortHelper
    {
        public static async Task<bool> IsSerialPortPresentedAsync(string serialPortName)
        {
            var aqsFilter = SerialDevice.GetDeviceSelector(serialPortName);
            var deviceInformation = (await DeviceInformation.FindAllAsync(aqsFilter)).FirstOrDefault();
            return deviceInformation != null;
        }
    }
}