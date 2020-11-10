using Windows.UI.Xaml.Controls;

namespace KioskApp.Controls
{
    public sealed partial class InputErrorFlyoutContent : UserControl
    {
        public InputErrorFlyoutContent(string errorMessage)
        {
            ErrorMessage = errorMessage;

            InitializeComponent();
        }

        public string ErrorMessage { get; set; }
    }
}