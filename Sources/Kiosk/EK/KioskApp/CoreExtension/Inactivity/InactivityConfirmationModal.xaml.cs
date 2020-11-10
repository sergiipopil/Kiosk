using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Inactivity;
using KioskBrains.Kiosk.Core.Languages;

namespace KioskApp.CoreExtension.Inactivity
{
    public sealed partial class InactivityConfirmationModal : UserControl, IDisposable
    {
        public InactivityConfirmationModalModel Model { get; }

        public InactivityConfirmationModal(InactivityConfirmationModalModel model)
        {
            Assure.ArgumentNotNull(model, nameof(model));

            Model = model;

            Question = LanguageManager.Current.GetLocalizedString("InactivityConfirmationModal_Question");
            ConfirmButtonText = LanguageManager.Current.GetLocalizedString("InactivityConfirmationModal_ConfirmButtonText");

            InitializeComponent();
        }

        public string Question { get; set; }

        public string ConfirmButtonText { get; set; }

        public void StartCountdown()
        {
            CountdownElement.Start();
        }

        private void CountdownElement_OnRunOut(object sender, EventArgs e)
        {
            Model.OnCountdownRunOut();
        }

        public void Dispose()
        {
            CountdownElement.Stop();
        }

        // Only for testing with mouse.
        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            InactivityManager.Current.RegisterCustomInteraction(InactivityCustomInteractionSourceEnum.NonTouchInput);
        }
    }
}