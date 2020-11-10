using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Waf.Managers.Common;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Managers
{
    public class SystemCustomerProvider : IWafManager
    {
        private readonly KioskBrainsContext _dbContext;

        public SystemCustomerProvider(
            KioskBrainsContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<int> GetSystemCustomerIdAsync(CancellationToken cancellationToken)
        {
            return _dbContext.Customers
                .Where(x => x.IsSystem)
                .Select(x => x.Id)
                .FirstAsync(cancellationToken);
        }
    }
}