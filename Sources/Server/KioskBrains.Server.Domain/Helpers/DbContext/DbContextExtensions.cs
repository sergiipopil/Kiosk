using System.Threading.Tasks;
using Dapper;
using KioskBrains.Server.Domain.Entities.DbStorage;

namespace KioskBrains.Server.Domain.Helpers.DbContext
{
    public static class DbContextExtensions
    {
        public static Task<long> GetNextSequenceValueAsync(this KioskBrainsContext dbContext, string sequenceName)
        {
            return dbContext.Connection.ExecuteScalarAsync<long>($"SELECT NEXT VALUE FOR {sequenceName}");
        }
    }
}