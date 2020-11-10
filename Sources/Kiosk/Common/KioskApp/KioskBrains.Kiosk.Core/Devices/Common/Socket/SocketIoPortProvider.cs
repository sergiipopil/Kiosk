using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Devices.Exceptions;

namespace KioskBrains.Kiosk.Core.Devices.Common.Socket
{
    public class SocketIoPortProvider : IDeviceIoPortProvider
    {
        private readonly SocketSettings _socketSettings;

        public SocketIoPortProvider(SocketSettings socketSettings)
        {
            Assure.ArgumentNotNull(socketSettings, nameof(socketSettings));

            _socketSettings = socketSettings;
        }

        public async Task<IDeviceIoPort> OpenDeviceIoPortAsync(CancellationToken cancellationToken)
        {
            var socket = new StreamSocket();

            try
            {
                await socket.ConnectAsync(_socketSettings.RemoteHostName, _socketSettings.RemoteServiceName)
                    // todo: cancellation token doesn't work here for some reason - investigate (real timeout is 20 seconds)
                    .AsTask(cancellationToken);
            }
            catch (Exception ex)
            {
                // dispose socket if connection fails
                socket.Dispose();

                if (ex is OperationCanceledException)
                {
                    throw;
                }

                var socketErrorStatus = SocketError.GetStatus(ex.HResult);
                if (socketErrorStatus == SocketErrorStatus.Unknown)
                {
                    throw;
                }

                throw new DeviceIoPortCommunicationException($"'{nameof(StreamSocket.ConnectAsync)}' failed with '{socketErrorStatus}'.", ex);
            }

            return new SocketIoPort(socket);
        }
    }
}