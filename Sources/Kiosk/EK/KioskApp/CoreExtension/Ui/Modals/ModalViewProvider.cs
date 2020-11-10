using System;
using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Modals;

namespace KioskApp.CoreExtension.Ui.Modals
{
    public class ModalViewProvider : IModalViewProvider
    {
        public UserControl GetModalContentContainer(UserControl modalContent, Action onCancelled, string titleLabel, bool centerTitleHorizontally, bool showCancelButton)
        {
            return new ModalContentContainer(modalContent, onCancelled, titleLabel, centerTitleHorizontally, showCancelButton);
        }
    }
}