using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Controls
{
    public sealed partial class RightContentHeaderBackButton : UserControl
    {
        public RightContentHeaderBackButton()
        {
            InitializeComponent();
        }

        #region Command

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command), typeof(ICommand), typeof(RightContentHeaderBackButton), new PropertyMetadata(default(ICommand)));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        #endregion

        public event RoutedEventHandler Click;

        private void OnClick(RoutedEventArgs e)
        {
            Click?.Invoke(this, e);
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            OnClick(e);
        }
    }
}