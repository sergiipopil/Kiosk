using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Controls
{
    public sealed partial class SoonFlyout : UserControl
    {
        public SoonFlyout()
        {
            InitializeComponent();
        }

        public static void ShowAt(FrameworkElement target)
        {
            ThreadHelper.EnsureUiThread();

            var flyout = new Flyout()
                {
                    Content = new SoonFlyout(),
                    Placement = FlyoutPlacementMode.Right,
                    FlyoutPresenterStyle = ResourceHelper.GetStaticResourceFromUIThread<Style>("TransparentFlyoutPresenterStyle"),
                };

            flyout.ShowAt(target);
        }
    }
}