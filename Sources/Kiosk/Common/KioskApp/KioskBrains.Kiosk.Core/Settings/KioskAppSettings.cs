using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using KioskBrains.Common.Api;
using KioskBrains.Common.Constants;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.KioskConfiguration;
using KioskBrains.Common.KioskState;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Components.Initialization;
using KioskBrains.Kiosk.Core.Components.Operations;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Core.ServerApi;
using KioskBrains.Kiosk.Core.Storage;
using KioskBrains.Kiosk.Helpers.Applications;

namespace KioskBrains.Kiosk.Core.Settings
{
    public class KioskAppSettings : ComponentBase, IKioskAppSettingsContract
    {
        public override bool IsStateMonitorable => false;

        protected override Type[] GetSupportedContracts()
        {
            return new[]
                {
                    typeof(IKioskAppSettingsContract),
                };
        }

        protected override async Task<ComponentInitializeResponse> InitializeAsync(ComponentInitializeRequest request, ComponentOperationContext context)
        {
            State.KioskAppVersion = VersionHelper.AppVersionString;

            // app configuration
            var _appConfiguration = await AppDataStorage.Current.LoadRecordAsync<KioskAppConfiguration>(
                KioskFolderNames.AppData_Configuration,
                KioskFileNames.KioskAppConfigurationWithoutExtension,
                true,
                CancellationToken.None);
            Assure.CheckFlowState(_appConfiguration.ServerUri != null, $"'{nameof(KioskFileNames.KioskAppConfigurationWithoutExtension)}.{nameof(KioskAppConfiguration.ServerUri)}' is null.");

            State.ServerUri = _appConfiguration.ServerUri;

            // serial key
            string serialKey;
            try
            {
                var kioskPublicFolder = await StorageHelper.GetKioskRootFolderAsync();
                var storageFile = await kioskPublicFolder.GetFileAsync(KioskFileNames.Key);
                serialKey = await FileIO.ReadTextAsync(storageFile, UnicodeEncoding.Utf8);
            }
            catch (FileNotFoundException)
            {
                Status.SetSelfStatus(ComponentStatusCodeEnum.Error, $"'{KioskFileNames.Key}' was not found.");
                return ComponentInitializeResponse.GetError();
            }

            // init server API proxy
            ServerApiProxy.Current.Initialize(State.ServerUri, serialKey);

            // get kiosk configuration
            var loadedConfiguration = await GetKioskConfigurationAsync(context);
            var kioskConfiguration = loadedConfiguration.Configuration;
            Assure.CheckFlowState(kioskConfiguration != null, "Loaded kiosk configuration is null.");

            // state properties
            State.KioskConfiguration = kioskConfiguration;
            // ReSharper disable PossibleNullReferenceException
            State.KioskId = kioskConfiguration.KioskId;
            // ReSharper restore PossibleNullReferenceException
            State.SupportPhone = kioskConfiguration.SupportPhone;
            State.KioskAddress = kioskConfiguration.KioskAddress;

            // init languages
            LanguageManager.Current.Initialize(kioskConfiguration.LanguageCodes);

            if (loadedConfiguration.IsLoadedFromCache)
            {
                Status.SetSelfStatus(ComponentStatusCodeEnum.Warning, "Loaded from cache.");
            }
            else
            {
                Status.SetSelfStatus(ComponentStatusCodeEnum.Ok, null);
            }

            return ComponentInitializeResponse.GetSuccess();
        }

        private const string CachedConfigurationRecordName = "KioskConfiguration";

        private readonly PersistentCacheManager<KioskConfiguration> _persistentCacheManager
            = new PersistentCacheManager<KioskConfiguration>(CachedConfigurationRecordName);

        private const int GetInitialConfigurationMaxTries = 5;

        private readonly TimeSpan GetInitialConfigurationNextTryDelay = TimeSpan.FromSeconds(10);

        // todo: request configuration periodically? (considering that kiosk will not be restarted long period of time)
        private async Task<(KioskConfiguration Configuration, bool IsLoadedFromCache)> GetKioskConfigurationAsync(ComponentOperationContext context)
        {
            var tryNumber = 0;

            GetConfigurationTry:
            try
            {
                tryNumber++;

                var response = await ServerApiProxy.Current.GetKioskConfigurationAsync(new EmptyRequest());
                Assure.CheckFlowState(response.KioskConfiguration != null, $"Received '{nameof(KioskConfiguration)}' is null.");

                var kioskConfiguration = response.KioskConfiguration;

                // cache configuration for case of Internet absence
                await _persistentCacheManager.SaveAsync(kioskConfiguration, context);

                return (kioskConfiguration, false);
            }
            catch (Exception ex)
            {
                if (tryNumber == 1)
                {
                    context.Log.Error(LogContextEnum.Communication, $"Kiosk configuration request failed (try {tryNumber}).", ex);
                }

                if (tryNumber < GetInitialConfigurationMaxTries)
                {
                    await Task.Delay(GetInitialConfigurationNextTryDelay);
                    goto GetConfigurationTry;
                }

                // try to get cached configuration
                var cachedKioskConfiguration = await _persistentCacheManager.LoadAsync(context);
                if (cachedKioskConfiguration == null)
                {
                    throw;
                }

                context.Log.Warning(LogContextEnum.Component, "Kiosk configuration was loaded from cache.");
                return (cachedKioskConfiguration, true);
            }
        }

        #region IKioskAppSettingsContract

        public KioskAppSettingsState State { get; } = new KioskAppSettingsState();

        #endregion
    }
}