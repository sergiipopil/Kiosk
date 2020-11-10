using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Server.Domain.Actions.Common.Models;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateUpdateDetails
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalCentralBankExchangeRateUpdateDetailsDelete : WafActionDelete<CommonDetailsDeleteRequest, EmptyResponse>
    {
        private readonly CurrentUser _currentUser;
        private readonly KioskBrainsContext _dbContext;

        public PortalCentralBankExchangeRateUpdateDetailsDelete(
            CurrentUser currentUser,
            KioskBrainsContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
        }

        public override async Task<EmptyResponse> ExecuteAsync(CommonDetailsDeleteRequest request)
        {
            var deletingItem = await _dbContext.CentralBankExchangeRateUpdates
                .Where(x => x.Id == request.Id)
                .FirstOrDefaultAsync();
            if (deletingItem == null)
            {
                throw EntityNotFoundException.Create<CentralBankExchangeRate>(request.Id);
            }

            // remove
            _dbContext.CentralBankExchangeRateUpdates.Remove(deletingItem);
            await _dbContext.SaveChangesAsync();

            return new EmptyResponse();
        }
    }
}