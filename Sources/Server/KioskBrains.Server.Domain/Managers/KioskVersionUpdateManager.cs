using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Waf.Managers.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace KioskBrains.Server.Domain.Managers
{
    public class KioskVersionUpdateManager : IWafManager
    {
        private readonly KioskBrainsContext _dbContext;

        private readonly IMemoryCache _memoryCache;

        public KioskVersionUpdateManager(
            KioskBrainsContext dbContext,
            IMemoryCache memoryCache)
        {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
        }

        public async Task<string> GetUpdateUrlAsync(KioskApplicationTypeEnum kioskApplicationType, string versionName)
        {
            var versionCacheKey = $"KioskVersionUpdateUrl-{kioskApplicationType}-{versionName}";
            if (_memoryCache.TryGetValue<string>(versionCacheKey, out var updateUrl))
            {
                return updateUrl;
            }

            // race conditions are not handled at the moment

            var versionUpdateData = await _dbContext.KioskVersionUpdates
                .Where(x => x.ApplicationType == kioskApplicationType
                            && x.VersionName == versionName)
                .Select(x => new
                    {
                        x.UpdateUrl,
                    })
                .FirstOrDefaultAsync();
            updateUrl = versionUpdateData?.UpdateUrl;
            if (string.IsNullOrEmpty(updateUrl))
            {
                return null;
            }

            _memoryCache.Set(versionCacheKey, updateUrl);

            return updateUrl;
        }
    }
}