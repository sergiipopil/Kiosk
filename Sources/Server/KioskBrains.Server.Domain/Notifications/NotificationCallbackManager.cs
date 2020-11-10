using System;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Managers.Common;

namespace KioskBrains.Server.Domain.Notifications
{
    /// <summary>
    /// Separate from <see cref="NotificationManager"/> since callbacks are processed in anonymous context (without dependency on <see cref="CurrentUser"/>).
    /// </summary>
    public class NotificationCallbackManager : IWafManager
    {
        private readonly KioskBrainsContext _dbContext;

        public NotificationCallbackManager(KioskBrainsContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task LogReceivedCallbackMessageAsync(string channel, string messageJson)
        {
            _dbContext.NotificationCallbackLogRecords.Add(new NotificationCallbackLogRecord()
                {
                    ReceivedOnUtc = DateTime.UtcNow,
                    Channel = channel,
                    MessageJson = messageJson,
                });
            await _dbContext.SaveChangesAsync();
        }
    }
}