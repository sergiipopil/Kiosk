using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Common.EK.Transactions;
using KioskBrains.Common.KioskState;
using KioskBrains.Common.Transactions;
using KioskBrains.Server.Common.Log;
using KioskBrains.Server.Common.Notifications;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskBrains.Server.Domain.Actions.Kiosk.KioskSync
{
    [AuthorizeUser(UserRoleEnum.KioskApp)]
    public class KioskSyncPost : WafActionPost<KioskSyncPostRequest, KioskSyncPostResponse>
    {
        private readonly CurrentUser _currentUser;

        private readonly KioskBrainsContext _dbContext;

        private readonly INotificationManager _notificationManager;

        private readonly ILogger<KioskSyncPost> _logger;

        public KioskSyncPost(
            CurrentUser currentUser,
            KioskBrainsContext dbContext,
            INotificationManager notificationManager,
            ILogger<KioskSyncPost> logger)
        {
            _currentUser = currentUser;
            _dbContext = dbContext;
            _notificationManager = notificationManager;
            _logger = logger;
        }

        public override async Task<KioskSyncPostResponse> ExecuteAsync(KioskSyncPostRequest request)
        {
            var kioskId = _currentUser.Id;

            var utcNow = DateTime.UtcNow;

            var notifications = new List<Notification>();

            // kiosk state
            await VerifyKioskAndProcessKioskStateAsync(kioskId, request.KioskState, utcNow, notifications);

            // transactions
            ProcessTransactions(kioskId, request.TransactionContainersJson, utcNow);

            // log records
            ProcessLogRecords(kioskId, request.LogRecordsJson);

            await _dbContext.SaveChangesAsync();

            // notifications
            if (notifications.Count > 0)
            {
                await _notificationManager.SendNotificationsAsync(
                    notifications.ToArray(),
                    CancellationToken.None);
            }

            var response = new KioskSyncPostResponse();
            return response;
        }

        #region Kiosk State

        private async Task VerifyKioskAndProcessKioskStateAsync(
            int kioskId,
            KioskMonitorableState newKioskState,
            DateTime utcNow,
            IList<Notification> notifications)
        {
            // check if the kiosk exists and update ping time
            // DbContext is used to stay in SQL transaction
            // todo: add/test Dapper/EF common transaction and update LastPingedOnUtc via Dapper
            var kiosk = await _dbContext.Kiosks
                .Include(x => x.CurrentState)
                .ThenInclude(x => x.ComponentsStatuses)
                .Where(x => x.Id == kioskId)
                .FirstOrDefaultAsync();
            if (kiosk == null)
            {
                throw EntityNotFoundException.Create<Entities.Kiosk>(kioskId);
            }

            kiosk.LastPingedOnUtc = utcNow;

            if (newKioskState == null)
            {
                return;
            }

            // create current kiosk state if it doesn't exist already
            if (kiosk.CurrentState == null)
            {
                kiosk.CurrentState = new KioskState();
            }

            var kioskState = kiosk.CurrentState;

            // apply new kiosk state
            kioskState.KioskVersion = newKioskState.KioskVersion;
            kioskState.LocalTime = newKioskState.LocalTime;
            kioskState.TimeUtc = utcNow;


            // kiosk's components' statuses
            kioskState.ComponentsStatuses = kioskState.ComponentsStatuses ?? new List<KioskStateComponentInfo>();
            newKioskState.ComponentMonitorableStates = newKioskState.ComponentMonitorableStates ?? new ComponentMonitorableState[] { };

            var existingComponentsStatusesByNames = kioskState.ComponentsStatuses.ToDictionary(x => x.ComponentName);
            var newComponentsStatusesByNames = newKioskState.ComponentMonitorableStates.ToDictionary(x => x.ComponentName);

            var deletedComponentsStatusesNames = existingComponentsStatusesByNames.Keys.Except(newComponentsStatusesByNames.Keys).ToHashSet();
            var addedComponentsStatusesNames = newComponentsStatusesByNames.Keys.Except(existingComponentsStatusesByNames.Keys).ToHashSet();

            // delete
            DeleteComponentStatuses(
                _dbContext,
                kioskState,
                kioskState.ComponentsStatuses
                    .Where(x => deletedComponentsStatusesNames.Contains(x.ComponentName))
                    .ToArray());

            // update
            foreach (var componentStatus in kioskState.ComponentsStatuses)
            {
                var newComponentStatus = newComponentsStatusesByNames[componentStatus.ComponentName];

                // add notification if component switched to non-operational status
                var newStatus = newComponentStatus.Status?.Code ?? ComponentStatusCodeEnum.Undefined;
                if (componentStatus.StatusCode.IsOperational() && !newStatus.IsOperational())
                {
                    var messageBuilder = new StringBuilder($"Kiosk #{kioskId}: component {componentStatus.ComponentName} switched to non-operational status {newStatus}.");
                    if (!string.IsNullOrWhiteSpace(newComponentStatus.Status?.Message))
                    {
                        messageBuilder.Append($"\n{newComponentStatus.Status.Message}");
                    }

                    notifications.Add(new Notification
                    {
                        CustomerId = _currentUser.CustomerId,
                        Type = NotificationTypeEnum.Service,
                        Message = messageBuilder.ToString(),
                    });
                }

                componentStatus.StatusCode = newStatus;
                componentStatus.StatusMessage = newComponentStatus.Status?.Message;
                componentStatus.SpecificMonitorableStateJson = newComponentStatus.SpecificMonitorableStateJson;
            }

            // add
            var addedComponentsStatuses = newComponentsStatusesByNames
                .Where(x => addedComponentsStatusesNames.Contains(x.Key))
                .Select(x => x.Value)
                .Select(KioskStateComponentInfo.FromComponentMonitorableState);
            kioskState.ComponentsStatuses.AddRange(addedComponentsStatuses);
        }

        private static void DeleteComponentStatuses(
            KioskBrainsContext dbContext,
            KioskState kioskState,
            KioskStateComponentInfo[] componentStatuses)
        {
            foreach (var componentStatus in componentStatuses)
            {
                dbContext.KioskStateComponentInfos.Remove(componentStatus);
                kioskState.ComponentsStatuses.Remove(componentStatus);
            }
        }

        #endregion

        #region Transactions

        private void ProcessTransactions(int kioskId, string transactionContainersJson, DateTime utcNow)
        {
            if (transactionContainersJson == null)
            {
                return;
            }

            try
            {
                var transactionContainers = JsonConvert.DeserializeObject<TransactionContainer[]>(transactionContainersJson);
                foreach (var transactionContainer in transactionContainers)
                {
                    try
                    {
                        // todo: prevent multiple records with the same UniqueId

                        switch (transactionContainer.Workflow)
                        {
                            case TransactionWorkflowEnum.Ek:
                                var apiEkTransaction = JsonConvert.DeserializeObject<EkTransaction>(transactionContainer.TransactionJson);
                                var ekTransaction = Entities.EK.EkTransaction.FromApiModel(kioskId, utcNow, apiEkTransaction);
                                _dbContext.EkTransactions.Add(ekTransaction);
                                break;
                            default:
                                _logger.LogError(LoggingEvents.NotSupported, $"'{nameof(TransactionStatusEnum)}.{transactionContainer.Workflow}' is not supported.");
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(LoggingEvents.UnhandledException, ex, $"Transaction handling failed: '{transactionContainer.TransactionJson}'.");
                        // continue with other transactions
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, ex, $"Transactions handling failed: '{transactionContainersJson}'.");
            }
        }

        #endregion

        #region Log Records

        private void ProcessLogRecords(int kioskId, string logRecordsJson)
        {
            if (logRecordsJson == null)
            {
                return;
            }

            try
            {
                var apiLogRecords = JsonConvert.DeserializeObject<KioskBrains.Common.Logging.LogRecord[]>(logRecordsJson);
                var logRecords = apiLogRecords.Select(x => LogRecord.FromApiModel(kioskId, x));
                _dbContext.LogRecords.AddRange(logRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.UnhandledException, ex, $"Log records handling failed: '{logRecordsJson}'.");
            }
        }

        #endregion
    }
}