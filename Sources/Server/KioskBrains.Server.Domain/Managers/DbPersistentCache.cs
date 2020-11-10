using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Server.Common.Cache;
using Microsoft.EntityFrameworkCore;
using db = KioskBrains.Server.Domain.Entities.DbStorage.KioskBrainsContext;
using item = KioskBrains.Server.Domain.Entities.DbPersistentCacheItem;

namespace KioskBrains.Server.Domain.Managers
{
    public class DbPersistentCache : IPersistentCache
    {
        private readonly db _dbContext;

        public DbPersistentCache(
            db dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<string> GetValueAsync(string key, CancellationToken cancellationToken)
        {
            return _dbContext.DbPersistentCacheItems
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static readonly string UpsertItemSql = $@"
MERGE {nameof(db.DbPersistentCacheItems)} AS [Target] 
USING (SELECT @Key AS [{nameof(item.Key)}], @Value AS {nameof(item.Value)}) AS [Source] ON [Target].[{nameof(item.Key)}] = [Source].[{nameof(item.Key)}]
WHEN MATCHED THEN UPDATE SET [Target].{nameof(item.Value)} = [Source].{nameof(item.Value)}
WHEN NOT MATCHED THEN INSERT ([{nameof(item.Key)}], {nameof(item.Value)}) VALUES ([Source].[{nameof(item.Key)}], [Source].{nameof(item.Value)});";

        public async Task SetValueAsync(string key, string value, CancellationToken cancellationToken)
        {
            await _dbContext.Database.ExecuteSqlCommandAsync(
                UpsertItemSql,
                new[]
                    {
                        new SqlParameter("@Key", SqlDbType.NVarChar)
                            {
                                Value = key,
                            },
                        new SqlParameter("@Value", SqlDbType.NVarChar)
                            {
                                Value = value,
                            },
                    },
                cancellationToken);
        }
    }
}