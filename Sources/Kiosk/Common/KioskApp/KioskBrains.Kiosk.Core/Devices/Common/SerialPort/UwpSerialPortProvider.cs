using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Devices.Exceptions;

namespace KioskBrains.Kiosk.Core.Devices.Common.SerialPort
{
    public class UwpSerialPortProvider : IDeviceIoPortProvider
    {
        private readonly SerialPortSettings _serialPortSettings;

        public UwpSerialPortProvider(SerialPortSettings serialPortSettings)
        {
            Assure.ArgumentNotNull(serialPortSettings, nameof(serialPortSettings));

            _serialPortSettings = serialPortSettings;
        }

        public async Task<IDeviceIoPort> OpenDeviceIoPortAsync(CancellationToken cancellationToken)
        {
            var serialDevice = await GetSerialDeviceAsync(_serialPortSettings.SerialPortName);
            try
            {
                // init serial device
                var tryNumber = 1;
                while (true)
                {
                    try
                    {
                        // set of BaudRate sometimes fails
                        // todo: understand when and why
                        serialDevice.BaudRate = _serialPortSettings.BaudRate;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(LogContextEnum.Device, $"Set of '{nameof(serialDevice.BaudRate)}' failed (try #{tryNumber}).", ex);
                        if (tryNumber == 3)
                        {
                            throw;
                        }
                        tryNumber++;
                        // small delay before next try
                        await Task.Delay(TimeSpan.FromMilliseconds(100), cancellationToken);
                    }
                }
                serialDevice.StopBits = _serialPortSettings.StopBits;
                serialDevice.DataBits = _serialPortSettings.DataBits;
                serialDevice.Parity = _serialPortSettings.Parity;
                serialDevice.ReadTimeout = _serialPortSettings.ReadTimeout;
                serialDevice.WriteTimeout = _serialPortSettings.WriteTimeout;

                return new UwpSerialPort(serialDevice);
            }
            catch
            {
                // dispose serial device if construction fails
                serialDevice.Dispose();
                throw;
            }
        }

        private const int MaxPortOpenningTries = 3;

        private static readonly TimeSpan PortOpenningTryDelay = TimeSpan.FromMilliseconds(500);

        private async Task<SerialDevice> GetSerialDeviceAsync(string serialPortName)
        {
            Assure.CheckFlowState(!string.IsNullOrEmpty(serialPortName), $"Empty '{nameof(serialPortName)}'.");

            // ReSharper disable AssignNullToNotNullAttribute
            var aqsFilter = SerialDevice.GetDeviceSelector(serialPortName);
            // ReSharper restore AssignNullToNotNullAttribute
            var deviceInformation = (await DeviceInformation.FindAllAsync(aqsFilter)).FirstOrDefault();
            if (deviceInformation == null)
            {
                throw new DeviceIoPortInitializationException($"Serial device on '{serialPortName}' port was not found.");
            }

            var portOpenningTry = 0;
            GetSerialDeviceTry:
            portOpenningTry++;

            var serialDevice = await SerialDevice.FromIdAsync(deviceInformation.Id);
            if (serialDevice != null)
            {
                return serialDevice;
            }

            var deviceAccessStatus = DeviceAccessInformation.CreateFromId(deviceInformation.Id).CurrentStatus;
            switch (deviceAccessStatus)
            {
                case DeviceAccessStatus.DeniedByUser:
                    throw new DeviceIoPortInitializationException($"Access to device on '{serialPortName}' port was blocked by the user.");
                case DeviceAccessStatus.DeniedBySystem:
                    // This status is most likely caused by app permissions (did not declare the device in the app's package.appxmanifest)
                    // This status does not cover the case where the device is already opened by another app.
                    throw new DeviceIoPortInitializationException($"Access to device on '{serialPortName}' was blocked by the system.");
                default:
                    // Sometimes serial port is not freed fast after previous Dispose. Retry.
                    var message = $"Access to device on '{serialPortName}' was blocked with unknown error (try {portOpenningTry}). Possibly opened by another app.";
                    if (portOpenningTry < MaxPortOpenningTries)
                    {
                        Log.Error(LogContextEnum.Device, message);
                        await Task.Delay(PortOpenningTryDelay);
                        goto GetSerialDeviceTry;
                    }
                    throw new DeviceIoPortInitializationException(message);
            }
        }
    }
}