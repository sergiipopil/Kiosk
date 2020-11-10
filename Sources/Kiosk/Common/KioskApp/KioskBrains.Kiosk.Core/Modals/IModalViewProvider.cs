using System;
using Windows.UI.Xaml.Controls;

namespace KioskBrains.Kiosk.Core.Modals
{
    public interface IModalViewProvider
    {
        UserControl GetModalContentContainer(UserControl modalContent, Action onCancelled, string titleLabel, bool centerTitleHorizontally, bool showCancelButton);
    }
}