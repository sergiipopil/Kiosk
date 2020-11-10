using System.Threading.Tasks;
using KioskBrains.Common.Constants;
using KioskBrains.Kiosk.Core.Components.Operations;

namespace KioskBrains.Kiosk.Core.Storage
{
    public class PersistentCacheManager<TCacheRecord>
        where TCacheRecord : class, new()
    {
        public PersistentCacheManager(string cacheRecordName)
        {
            _statePersistenceManager = new StatePersistenceManager<TCacheRecord>(
                cacheRecordName,
                state => state,
                KioskFolderNames.AppData_Cache);
        }

        private readonly StatePersistenceManager<TCacheRecord> _statePersistenceManager;

        public Task<TCacheRecord> LoadAsync(ComponentOperationContext context)
        {
            return _statePersistenceManager.LoadAsync(context);
        }

        /// <returns>'true' if record was saved successfully, 'false' is not.</returns>
        public Task<bool> SaveAsync(TCacheRecord cacheRecord, ComponentOperationContext context)
        {
            return _statePersistenceManager.SaveAsync(cacheRecord, context);
        }
    }
}