using System.Threading;
using System.Threading.Tasks;

namespace KioskBrains.Server.Common.Cache
{
    public interface IPersistentCache
    {
        Task<string> GetValueAsync(string key, CancellationToken cancellationToken);

        Task SetValueAsync(string key, string value, CancellationToken cancellationToken);
    }
}