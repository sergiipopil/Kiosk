using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Helpers.Ui
{
    public static class ResourceHelper
    {
        public static async Task<TResource> GetStaticResourceAsync<TResource>(string resourceKey)
        {
            return await ThreadHelper.GetFromUiThreadAsync(() => GetStaticResourceFromUIThread<TResource>(resourceKey));
        }

        public static TResource GetStaticResourceFromUIThread<TResource>(string resourceKey)
        {
            Assure.ArgumentNotNull(resourceKey, nameof(resourceKey));
            try
            {
                return (TResource)Application.Current.Resources[resourceKey];
            }
            catch (Exception ex)
            {
                Log.Error(LogContextEnum.Application, $"Obtaining of static resource '{resourceKey}' has failed.", ex);
                return default(TResource);
            }
        }
    }
}