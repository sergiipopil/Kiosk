using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Api.CarTree;
using KioskBrains.Common.EK.KioskConfiguration;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Settings;
using Newtonsoft.Json;

namespace KioskApp.CoreExtension.Application
{
    public static class EkSettingsHelper
    {
        private static EkSettings _ekSettings;

        private static readonly object _ekSettingsLocker = new object();

        private static void PrepareEkSettings()
        {
            lock (_ekSettingsLocker)
            {
                if (_ekSettings == null)
                {
                    var configuration = ComponentManager.Current.GetComponent<IKioskAppSettingsContract>().State.KioskConfiguration;
                    _ekSettings = JsonConvert.DeserializeObject<EkSettings>(configuration.SpecificSettingsJson);
                }
            }
        }

        public static EkProductCategory[] GetEuropeCategories()
        {
            PrepareEkSettings();

            return _ekSettings.EuropeCategories;
        }

        public static EkCarGroup[] GetCarModelTree()
        {
            PrepareEkSettings();

            return _ekSettings.CarModelTree;
        }
    }
}