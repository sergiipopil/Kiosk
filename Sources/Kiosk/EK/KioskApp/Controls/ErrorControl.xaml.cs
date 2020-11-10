using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Settings;

namespace KioskApp.Controls
{
    public sealed partial class ErrorControl : UserControl
    {
        public ErrorControl()
            : this(null)
        {
        }

        public ErrorControl(string errorMessage)
        {
            ErrorMessage = errorMessage;
            var kioskAppSettings = ComponentManager.Current.GetComponent<IKioskAppSettingsContract>(mandatory: false);
            SupportPhone = kioskAppSettings?.State.SupportPhone;

            InitializeComponent();
        }

        #region ErrorMessage

        public static readonly DependencyProperty ErrorMessageProperty = DependencyProperty.Register(
            nameof(ErrorMessage), typeof(string), typeof(ErrorControl), new PropertyMetadata(default(string)));

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }

        #endregion

        public string SupportPhone { get; set; }
    }
}