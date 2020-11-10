using System;
using KioskApp.CoreExtension.Inactivity;
using KioskApp.CoreExtension.Ui.Modals;
using KioskApp.Ek;
using KioskBrains.Kiosk.Core.Application;
using KioskBrains.Kiosk.Core.Inactivity;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.CoreExtension.Application
{
    public class KioskApplication : KioskApplicationBase
    {
        #region Singleton

        public static KioskApplication Current { get; } = new KioskApplication();

        private KioskApplication()
        {
        }

        #endregion

        protected override Type[] GetSupportedAppComponents()
        {
            return new[]
                {
                    // Workflow Launchers
                    typeof(EkApplication),
                };
        }

        protected override IKioskApplicationViewProvider GetApplicationViewProvider()
        {
            return new KioskApplicationViewProvider();
        }

        protected override IModalViewProvider GetModalViewProvider()
        {
            return new ModalViewProvider();
        }

        protected override IInactivityViewProvider GetInactivityViewProvider()
        {
            return new InactivityViewProvider();
        }

        protected override TimeSpan ApplicationInitializationDelay => TimeSpan.Zero;
    }
}