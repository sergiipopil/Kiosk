using Windows.Networking;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Kiosk.Core.Devices.Common.Socket
{
    public class SocketSettings
    {
        public HostName RemoteHostName { get; }

        public string RemoteServiceName { get; }

        public SocketSettings(HostName remoteHostName, string remoteServiceName)
        {
            Assure.ArgumentNotNull(remoteHostName, nameof(remoteHostName));
            Assure.ArgumentNotNull(remoteServiceName, nameof(remoteServiceName));

            RemoteHostName = remoteHostName;
            RemoteServiceName = remoteServiceName;
        }
    }
}