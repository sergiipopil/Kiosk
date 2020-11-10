using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Helpers.Threads;
using Buffer = Windows.Storage.Streams.Buffer;

namespace KioskBrains.Kiosk.Core.Devices.Common.Socket
{
    public class SocketIoPort : IDeviceIoPort
    {
        private readonly StreamSocket _socket;

        public SocketIoPort(StreamSocket socket)
        {
            Assure.ArgumentNotNull(socket, nameof(socket));

            _socket = socket;
        }

        // todo: copied from UwpSerialPort just in case - NEED TO BE TESTED SEPARATELY!
        // todo: WTF!!! understand why and fix - case PartialRead exception!
        // todo: looks like _serialDevice.InputStream.ReadAsync is not cancelled by .AsTask(cancellationToken) - so following read on the same SerialDevice could cause unexpected effects
        // todo: ConfigAwait(false)?
        public async void Dispose()
        {
            await ThreadHelper.RunInNewThreadAsync(() =>
                {
                    _socket.Dispose();
                    return Task.CompletedTask;
                });
        }

        public async Task<byte[]> ReadAsync(int bufferSize, InputStreamOptions inputStreamOptions, CancellationToken cancellationToken)
        {
            var readBuffer = new Buffer((uint)bufferSize);

            await _socket.InputStream.ReadAsync(readBuffer, readBuffer.Capacity, inputStreamOptions)
                .AsTask(cancellationToken);

            return readBuffer.ToArray();
        }

        public async Task WriteAsync(byte[] messageBytes, CancellationToken? cancellationToken)
        {
            // todo: copied from UwpSerialPort just in case - NEED TO BE TESTED SEPARATELY!
            // cancellation tokens don't work with write - so try to cancel manually before write
            cancellationToken?.ThrowIfCancellationRequested();

            await _socket.OutputStream.WriteAsync(messageBytes.AsBuffer());
        }
    }
}