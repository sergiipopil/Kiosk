using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Server.EK.Common.Cache;
using Microsoft.EntityFrameworkCore;
using db = KioskBrains.Server.Domain.Entities.DbStorage.KioskBrainsContext;
using item = KioskBrains.Server.Domain.Entities.EK.EkImageCacheItem;

namespace KioskBrains.Server.Domain.Managers
{
    public class EkImageCache : IEkImageCache
    {
        private readonly db _dbContext;

        public EkImageCache(
            db dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<string> GetValueAsync(string imageKey, CancellationToken cancellationToken)
        {
            return _dbContext.EkImageCacheItems
                .Where(x => x.ImageKey == imageKey)
                .Select(x => x.ImageUrl)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static readonly string UpsertItemSql = $@"
MERGE {nameof(db.EkImageCacheItems)} AS [Target] 
USING (SELECT @ImageKey AS [{nameof(item.ImageKey)}], @ImageUrl AS {nameof(item.ImageUrl)}) AS [Source] ON [Target].[{nameof(item.ImageKey)}] = [Source].[{nameof(item.ImageKey)}]
WHEN MATCHED THEN UPDATE SET [Target].{nameof(item.ImageUrl)} = [Source].{nameof(item.ImageUrl)}
WHEN NOT MATCHED THEN INSERT ([{nameof(item.ImageKey)}], {nameof(item.ImageUrl)}) VALUES ([Source].[{nameof(item.ImageKey)}], [Source].{nameof(item.ImageUrl)});";

        public async Task SetValueAsync(string imageKey, string imageUrl, CancellationToken cancellationToken)
        {
            await _dbContext.Database.ExecuteSqlCommandAsync(
                UpsertItemSql,
                new[]
                    {
                        new SqlParameter("@ImageKey", SqlDbType.NVarChar)
                            {
                                Value = imageKey,
                            },
                        new SqlParameter("@ImageUrl", SqlDbType.NVarChar)
                            {
                                Value = imageUrl,
                            },
                    },
                cancellationToken);
        }
    }
}