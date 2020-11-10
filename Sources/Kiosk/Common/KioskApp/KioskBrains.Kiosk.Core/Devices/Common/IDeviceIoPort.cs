using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace KioskBrains.Kiosk.Core.Devices.Common
{
    public interface IDeviceIoPort : IDisposable
    {
        Task<byte[]> ReadAsync(int bufferSize, InputStreamOptions inputStreamOptions, CancellationToken cancellationToken);

        Task WriteAsync(byte[] messageBytes, CancellationToken? cancellationToken);
    }
}