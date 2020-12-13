using KioskBrains.Common.EK.Api;
using KioskBrains.Common.EK.Api.CarTree;
using KioskBrains.Common.EK.KioskConfiguration;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Settings;
using Newtonsoft.Json;
using System;

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

        public static string GetModelFullNameByModelId(string modelId)
        {
            var model = GetModelAndNameByModelId(modelId);
            if (model!=null)
            if (!string.IsNullOrEmpty(modelId) && modelId != "0")
            {               
                return $" {model.ManufacturerName} {model.Name}";
            }
            return "";
        }

        public static string GetModelManufacturerNameByModelId(string modelId)
        {
            var model = GetModelAndNameByModelId(modelId);
            if (model != null)
                if (!string.IsNullOrEmpty(modelId) && modelId != "0")
                {
                    return $"{model.ManufacturerName}";
                }
            return "";
        }

        public static string GetModelManufacturerIdByModelId(string modelId)
        {
            var model = GetModelAndNameByModelId(modelId);
            if (model != null)
                if (!string.IsNullOrEmpty(modelId) && modelId != "0")
                {
                    return $"{model.ManufacturerId}";
                }
            return "";
        }

        public static EkCarModel GetModelAndNameByModelId(string modelId)
        {
            var tree = GetCarModelTree();
            if (!string.IsNullOrEmpty(modelId) && modelId != "0")
            {
                foreach (var g in tree)
                    foreach (var manufacturer in g.Manufacturers)
                        foreach (var model in manufacturer.CarModels)
                        {
                            if (model.Id.ToString() == modelId)
                            {
                                return new EkCarModel() { Id = model.Id, CarType = g.CarType, ManufacturerId = manufacturer.Id, Name = model.Name, ManufacturerName = manufacturer.Name};
                            }
                        }

            }
            return null;
        }
    }
}