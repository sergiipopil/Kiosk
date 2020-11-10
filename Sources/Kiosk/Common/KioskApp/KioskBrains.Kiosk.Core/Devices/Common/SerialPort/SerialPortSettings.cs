using System;
using Windows.Devices.SerialCommunication;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Kiosk.Core.Devices.Common.SerialPort
{
    public class SerialPortSettings
    {
        public string SerialPortName { get; }

        public uint BaudRate { get; }

        public SerialStopBitCount StopBits { get; }

        public SerialParity Parity { get; }

        public ushort DataBits { get; }

        public TimeSpan ReadTimeout { get; set; } = TimeSpan.FromMilliseconds(500);

        public TimeSpan WriteTimeout { get; set; } = TimeSpan.FromMilliseconds(500);

        public SerialPortSettings(
            string serialPortName,
            uint baudRate,
            SerialStopBitCount stopBits,
            SerialParity parity,
            ushort dataBits = 8)
        {
            Assure.ArgumentNotNull(serialPortName, nameof(serialPortName));

            SerialPortName = serialPortName;
            BaudRate = baudRate;
            StopBits = stopBits;
            Parity = parity;
            DataBits = dataBits;
        }
    }
}