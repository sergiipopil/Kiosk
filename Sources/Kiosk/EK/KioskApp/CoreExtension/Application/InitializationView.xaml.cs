using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Components.Initialization;

namespace KioskApp.CoreExtension.Application
{
    public sealed partial class InitializationView : UserControl
    {
        public ComponentInitializationLog InitializationLog { get; }

        public InitializationView(ComponentInitializationLog initializationLog)
        {
            Assure.ArgumentNotNull(initializationLog, nameof(initializationLog));

            InitializationLog = initializationLog;

            InitializeComponent();
        }
    }
}