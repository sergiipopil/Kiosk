using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Languages;

namespace KioskApp.CoreExtension.Ui.Modals
{
    public sealed partial class ModalContentContainer : UserControl
    {
        public ModalContentContainer(UserControl modalContent, Action onCancelled, string titleLabel, bool centerTitleHorizontally, bool showCancelButton)
        {
            ShowCancelButton = showCancelButton;
            OnCancelled = onCancelled;

            InitializeComponent();

            ContentContainer.Children.Add(modalContent);
        }

        public string CancelLabel { get; } = LanguageManager.Current.GetLocalizedString("Common_Cancel");

        public bool ShowCancelButton { get; }

        public Action OnCancelled { get; }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnCancelled?.Invoke();
        }
    }
}