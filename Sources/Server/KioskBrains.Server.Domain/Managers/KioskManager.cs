using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Common.Constants;
using KioskBrains.Common.EK.KioskConfiguration;
using KioskBrains.Common.KioskConfiguration;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Domain.Actions.Common.Models;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Server.EK.Common.Helpers;
using KioskBrains.Waf.Helpers.Exceptions;
using KioskBrains.Waf.Managers.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskBrains.Server.Domain.Managers
{
    public class KioskManager : IWafManager
    {
        private readonly KioskBrainsContext _dbContext;

        private readonly CurrentUser _currentUser;

        private readonly ILogger<KioskManager> _logger;

        public KioskManager(
            KioskBrainsContext dbContext,
            CurrentUser currentUser,
            ILogger<KioskManager> logger)
        {
            _dbContext = dbContext;
            _currentUser = currentUser;
            _logger = logger;
        }

        public IQueryable<Kiosk> GetActiveKiosksQuery()
        {
            return _dbContext.Kiosks
                .Where(x => x.Status == KioskStatusEnum.Active);
        }

        public async Task<KioskConfiguration> GetKioskConfigurationAsync(int kioskId)
        {
            var kiosk = await _dbContext.Kiosks
                .AsNoTracking()
                .Include(x => x.Address)
                .Include(x => x.Customer)
                .Where(x => x.Id == kioskId)
                .FirstOrDefaultAsync();
            if (kiosk == null)
            {
                throw EntityNotFoundException.Create<Kiosk>(kioskId);
            }

            var components = new List<ComponentConfiguration>();
            ParseAndAddComponentConfigurations(components, kiosk.WorkflowComponentConfigurationsJson, kiosk.Id, nameof(kiosk.WorkflowComponentConfigurationsJson));

            var kioskConfiguration = new KioskConfiguration()
            {
                KioskId = kiosk.Id,
                AppComponents = components.ToArray(),
                SupportPhone = kiosk.Customer.SupportPhone,
                LanguageCodes = GetConfigurationLanguageCodes(kiosk.CommaSeparatedLanguageCodes),
                KioskAddress = new KioskAddress()
                {
                    AddressLine1 = kiosk.Address.AddressLine1,
                    AddressLine2 = kiosk.Address.AddressLine2,
                    City = kiosk.Address.City,
                },
            };

            // app-specific settings
            object specificSettings = null;
            switch (kiosk.ApplicationType)
            {
                case KioskApplicationTypeEnum.EK:
                    specificSettings = new EkSettings()
                    {
                        EuropeCategories = EkCategoryHelper.GetEuropeCategories(),
                        CarModelTree = EkCategoryHelper.GetCarModelTree(),
                    };
                    break;
            }

            if (specificSettings != null)
            {
                kioskConfiguration.SpecificSettingsJson = JsonConvert.SerializeObject(specificSettings);
            }

            return kioskConfiguration;
        }

        private void ParseAndAddComponentConfigurations(
            List<ComponentConfiguration> components, string componentConfigurationsJson, int kioskId, string containerName)
        {
            try
            {
                if (!string.IsNullOrEmpty(componentConfigurationsJson))
                {
                    var componentConfigurations = JsonConvert.DeserializeObject<ComponentConfiguration[]>(componentConfigurationsJson);
                    if (componentConfigurations?.Length > 0)
                    {
                        components.AddRange(componentConfigurations);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.DataProcessingError, $"Parsing of {nameof(containerName)} for kiosk {kioskId} failed.", ex);
            }
        }

        private string[] GetConfigurationLanguageCodes(string commaSeparatedLanguageCodes)
        {
            return commaSeparatedLanguageCodes?.Split(',').Select(x => x.Trim()).ToArray()
                   ?? new[] { Languages.GlobalLanguageCode };
        }

        public IQueryable<Kiosk> GetCurrentUserKiosksQuery()
        {
            var query = GetActiveKiosksQuery();
            if (!_currentUser.IsGlobalPortalUser)
            {
                query = query
                    .Where(x => x.CustomerId == _currentUser.CustomerId);
            }

            return query;
        }

        public Task<ListOptionInt[]> GetKioskListOptionsAsync(int[] ids = null)
        {
            var query = GetCurrentUserKiosksQuery();

            if (ids != null && ids.Length > 0)
            {
                query = query.Where(x => ids.Contains(x.Id));
            }

            return query.Select(x => new ListOptionInt()
                {
                    Value = x.Id,
                    DisplayName = x.Id + " - " + ((x.Address.City + ", ") ?? "") + (x.Address.AddressLine1 ?? ""),
                })
                .ToArrayAsync();
        }

        public async Task VerifyCurrentUserKioskIdAsync(int kioskId)
        {
            var isAvailable = await GetActiveKiosksQuery()
                .Where(x => x.Id == kioskId)
                .AnyAsync();
            if (!isAvailable)
            {
                throw DataAuthorizationException.CreateInvalidEntityKey<Kiosk>(kioskId);
            }
        }
    }
}