using System;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.Helpers.Dates;
using KioskBrains.Server.Domain.Actions.Common.Models;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Contracts;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateUpdateDetails
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalCentralBankExchangeRateUpdateDetailsPost : WafActionPost<PortalCentralBankExchangeRateUpdateDetailsPostRequest, CommonDetailsPostResponse>
    {
        private readonly CurrentUser _currentUser;
        private readonly KioskBrainsContext _dbContext;

        public PortalCentralBankExchangeRateUpdateDetailsPost(
            CurrentUser currentUser,
            KioskBrainsContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
        }

        public override async Task<CommonDetailsPostResponse> ExecuteAsync(PortalCentralBankExchangeRateUpdateDetailsPostRequest request)
        {
            CentralBankExchangeRateUpdate entity;
            if (request.Form.Id == null)
            {
                entity = new CentralBankExchangeRateUpdate
                    {
                        CentralBankExchangeRateId = request.Form.CentralBankExchangeRateId
                    };
                _dbContext.CentralBankExchangeRateUpdates.Add(entity);
            }
            else
            {
                var entityId = request.Form.Id.Value;
                entity = await _dbContext.CentralBankExchangeRateUpdates
                    .Where(x => x.Id == entityId
                                && x.CentralBankExchangeRateId == request.Form.CentralBankExchangeRateId)
                    .FirstOrDefaultAsync();
                if (entity == null)
                {
                    throw EntityNotFoundException.Create<CentralBankExchangeRate>(entityId);
                }
            }

            // check if it's not duplicate by time
            var updateWithTheSameTimeExists = await _dbContext.CentralBankExchangeRateUpdates
                .Where(x => x.CentralBankExchangeRateId == entity.CentralBankExchangeRateId
                            && x.Id != request.Form.Id
                            && x.StartTime == request.Form.StartDateTime)
                .AnyAsync();
            if (updateWithTheSameTimeExists)
            {
                throw new InvalidOperationException($"A rate update with the same start date & time '{request.Form.StartDateTime.ToDateTimeString()}' already exists.");
            }

            UpdateFields(entity, request.Form);

            await _dbContext.SaveChangesAsync();

            return new CommonDetailsPostResponse
                {
                    Id = entity.Id,
                };
        }

        private void UpdateFields(CentralBankExchangeRateUpdate entity, PortalCentralBankExchangeRateUpdateDetailsForm form)
        {
            entity.StartTime = form.StartDateTime;
            entity.Rate = form.Rate.GetMandatoryValue();
        }
    }
}