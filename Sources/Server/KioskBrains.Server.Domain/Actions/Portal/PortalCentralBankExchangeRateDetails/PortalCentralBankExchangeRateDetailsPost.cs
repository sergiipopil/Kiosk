using System;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Actions.Common.Models;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Contracts;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalCentralBankExchangeRateDetails
{
    [AuthorizeUser(UserRoleEnum.CustomerAdmin, UserRoleEnum.CustomerSupport, UserRoleEnum.GlobalAdmin, UserRoleEnum.GlobalSupport)]
    public class PortalCentralBankExchangeRateDetailsPost : WafActionPost<PortalCentralBankExchangeRateDetailsPostRequest, CommonDetailsPostResponse>
    {
        private readonly CurrentUser _currentUser;
        private readonly KioskBrainsContext _dbContext;

        public PortalCentralBankExchangeRateDetailsPost(
            CurrentUser currentUser,
            KioskBrainsContext dbContext)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
        }

        public override async Task<CommonDetailsPostResponse> ExecuteAsync(PortalCentralBankExchangeRateDetailsPostRequest request)
        {
            var isNew = request.Form.Id == null;
            CentralBankExchangeRate entity;
            if (isNew)
            {
                // check if duplicate currency pair
                var isDuplicate = await _dbContext.CentralBankExchangeRates
                    .Where(x => x.LocalCurrencyCode == request.Form.LocalCurrencyCode
                                && x.ForeignCurrencyCode == request.Form.ForeignCurrencyCode)
                    .AnyAsync();
                if (isDuplicate)
                {
                    throw new InvalidOperationException("Central bank rate for such currency pair already exists.");
                }

                entity = new CentralBankExchangeRate();
                _dbContext.CentralBankExchangeRates.Add(entity);
            }
            else
            {
                var entityId = request.Form.Id.Value;
                entity = await _dbContext.CentralBankExchangeRates
                    .Where(x => x.Id == entityId)
                    .FirstOrDefaultAsync();
                if (entity == null)
                {
                    throw EntityNotFoundException.Create<CentralBankExchangeRate>(entityId);
                }
            }

            UpdateFields(entity, request.Form, isNew);

            await _dbContext.SaveChangesAsync();

            return new CommonDetailsPostResponse()
                {
                    Id = entity.Id,
                };
        }

        private void UpdateFields(CentralBankExchangeRate entity, PortalCentralBankExchangeRateDetailsForm form, bool isNew)
        {
            if (isNew)
            {
                entity.LocalCurrencyCode = form.LocalCurrencyCode;
                entity.ForeignCurrencyCode = form.ForeignCurrencyCode;
            }

            entity.DefaultOrder = form.DefaultOrder.GetMandatoryValue();
        }
    }
}