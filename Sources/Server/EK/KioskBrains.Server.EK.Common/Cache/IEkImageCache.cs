using System.Threading;
using System.Threading.Tasks;

namespace KioskBrains.Server.EK.Common.Cache
{
    public interface IEkImageCache
    {
        Task<string> GetValueAsync(string imageKey, CancellationToken cancellationToken);

        Task SetValueAsync(string imageKey, string imageUrl, CancellationToken cancellationToken);
    }
}