using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Search;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.Layouts;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Ek.Filters
{
    public sealed partial class SearchInput : UserControl
    {
        public SearchInput()
        {
            ControlKeyCommand = new RelayCommand(
                nameof(ControlKeyCommand),
                parameter => OnControlKeyCommand(parameter as string));

            InitializeComponent();
        }

        #region SearchProvider

        public static readonly DependencyProperty SearchProviderProperty = DependencyProperty.Register(
            nameof(SearchProvider), typeof(SearchProviderBase), typeof(SearchInput), new PropertyMetadata(default(SearchProviderBase)));

        public SearchProviderBase SearchProvider
        {
            get => (SearchProviderBase)GetValue(SearchProviderProperty);
            set => SetValue(SearchProviderProperty, value);
        }

        #endregion

        #region Instruction

        public static readonly DependencyProperty InstructionProperty = DependencyProperty.Register(
            nameof(Instruction), typeof(string), typeof(SearchInput), new PropertyMetadata(default(string)));

        public string Instruction
        {
            get => (string)GetValue(InstructionProperty);
            set => SetValue(InstructionProperty, value);
        }

        #endregion

        #region KeyboardLayoutProvider

        public static readonly DependencyProperty KeyboardLayoutProviderProperty = DependencyProperty.Register(
            nameof(KeyboardLayoutProvider), typeof(IVirtualKeyboardLayoutProvider), typeof(SearchInput), new PropertyMetadata(default(IVirtualKeyboardLayoutProvider), KeyboardLayoutProviderPropertyChangedCallback));

        private static void KeyboardLayoutProviderPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SearchInput)d).OnKeyboardLayoutProviderChanged();
        }

        public IVirtualKeyboardLayoutProvider KeyboardLayoutProvider
        {
            get => (IVirtualKeyboardLayoutProvider)GetValue(KeyboardLayoutProviderProperty);
            set => SetValue(KeyboardLayoutProviderProperty, value);
        }

        #endregion

        #region MaxLength

        public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register(
            nameof(MaxLength), typeof(int), typeof(SearchInput), new PropertyMetadata(100));

        public int MaxLength
        {
            get => (int)GetValue(MaxLengthProperty);
            set => SetValue(MaxLengthProperty, value);
        }

        #endregion

        private void OnKeyboardLayoutProviderChanged()
        {
            KioskBrains.Kiosk.Core.Ui.VirtualKeyboard.VirtualKeyboard.SetCustomLayoutProvider(SearchTextInput, KeyboardLayoutProvider);
            VirtualKeyboard.Target = SearchTextInput;
        }

        private void ClearSearch_OnClick(object sender, RoutedEventArgs e)
        {
            if (SearchProvider != null)
            {
                SearchProvider.Term = null;
            }
        }

        public ICommand ControlKeyCommand { get; }

        private void OnControlKeyCommand(string controlCommand)
        {
            switch (controlCommand)
            {
                case CommonControlCommands.Confirm:
                    SearchProvider?.SearchManualRunCommand.Execute(null);
                    break;
            }
        }
    }
}