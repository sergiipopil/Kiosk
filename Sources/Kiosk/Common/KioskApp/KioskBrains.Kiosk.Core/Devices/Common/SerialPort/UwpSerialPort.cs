using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Helpers.Threads;
using Buffer = Windows.Storage.Streams.Buffer;

namespace KioskBrains.Kiosk.Core.Devices.Common.SerialPort
{
    public class UwpSerialPort : IDeviceIoPort
    {
        private SerialDevice _serialDevice;

        public UwpSerialPort(SerialDevice serialDevice)
        {
            Assure.ArgumentNotNull(serialDevice, nameof(serialDevice));

            _serialDevice = serialDevice;
        }

        // todo: WTF!!! understand why and fix - case PartialRead exception!
        // todo: looks like _serialDevice.InputStream.ReadAsync is not cancelled by .AsTask(cancellationToken) - so following read on the same SerialDevice could cause unexpected effects
        // todo: ConfigAwait(false)?
        public async void Dispose()
        {
            await ThreadHelper.RunInNewThreadAsync(() =>
                {
                    _serialDevice.Dispose();
                    return Task.CompletedTask;
                });
            // todo: do we need this?
            _serialDevice = null;
        }

        public async Task<byte[]> ReadAsync(int bufferSize, InputStreamOptions inputStreamOptions, CancellationToken cancellationToken)
        {
            var readBuffer = new Buffer((uint)bufferSize);

            await _serialDevice.InputStream.ReadAsync(readBuffer, readBuffer.Capacity, inputStreamOptions)
                .AsTask(cancellationToken);

            return readBuffer.ToArray();
        }

        public async Task WriteAsync(byte[] messageBytes, CancellationToken? cancellationToken)
        {
            // cancellation tokens don't work with write - so try to cancel manually before write
            cancellationToken?.ThrowIfCancellationRequested();

            await _serialDevice.OutputStream.WriteAsync(messageBytes.AsBuffer());
        }
    }
}