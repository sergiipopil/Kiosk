using System.Threading;
using System.Threading.Tasks;

namespace KioskBrains.Kiosk.Core.Devices.Common
{
    public interface IDeviceIoPortProvider
    {
        Task<IDeviceIoPort> OpenDeviceIoPortAsync(CancellationToken cancellationToken);
    }
}