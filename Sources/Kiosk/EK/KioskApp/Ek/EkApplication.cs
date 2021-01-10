using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using KioskApp.Ek.AdvertisementDisplay;
using KioskBrains.Common.Helpers;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Components.Initialization;
using KioskBrains.Kiosk.Core.Components.Operations;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskApp.Ek
{
    /// <summary>
    /// Base app component, used:
    /// - to enable/disable app remotely
    /// - to send some extended monitorable state
    /// - to enable/disable advertisement display
    /// </summary>
    public class EkApplication : ComponentBase
    {
        public const string RoleName = "EK";

        public override bool IsStateMonitorable => false;

        protected override async Task<ComponentInitializeResponse> InitializeAsync(ComponentInitializeRequest request, ComponentOperationContext context)
        {
            Log.Info(LogContextEnum.Application, "EkApp initialize async start");
            var isAdvertisementDisplayDisabled = request.Settings.Get<bool?>("IsAdvertisementDisplayDisabled", mandatory: false) == true;
            if (!isAdvertisementDisplayDisabled)
            {
                await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Window.Current.Content = new AdvertisementDisplayView();
                    // You have to activate the window in order to show it later.
                    Window.Current.Activate();

                    var secondaryViewId = ApplicationView.GetForCurrentView().Id;

                    ThreadHelper.RunInUiThreadAsync(async () =>
                    {
                        Log.Info(LogContextEnum.Application, "EkApp RunInUiThreadAsync");
                        var isShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(secondaryViewId);
                        if (isShown)
                        {
                            Log.Info(LogContextEnum.Application, "Advertisement display was shown.");
                        }
                        else
                        {
                            Log.Error(LogContextEnum.Application, "Advertisement display was not shown.");
                        }
                        Log.Info(LogContextEnum.Application, "EkApp RunInUiThreadAsync end");
                    });
                });
            }

            return ComponentInitializeResponse.GetSuccess();
        }

        protected override Type[] GetSupportedContracts()
        {
            return new Type[0];
        }
    }
}