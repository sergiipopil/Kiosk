using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using KioskApp.Controls.Keyboards;
using KioskBrains.Kiosk.Core.Ui.Controls;
using KioskBrains.Kiosk.Core.Ui.VirtualKeyboard;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Controls
{
    public sealed partial class ValueInput : UserControl
    {
        public ValueInput()
        {
            InitializeComponent();
        }

        #region InputType

        public static readonly DependencyProperty InputTypeProperty = DependencyProperty.Register(
            nameof(InputType), typeof(ValueInputTypeEnum), typeof(ValueInput), new PropertyMetadata(default(ValueInputTypeEnum), InputTypePropertyChangedCallback));

        private static void InputTypePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ValueInput)d).OnInputTypeChanged();
        }

        public ValueInputTypeEnum InputType
        {
            get => (ValueInputTypeEnum)GetValue(InputTypeProperty);
            set => SetValue(InputTypeProperty, value);
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value), typeof(string), typeof(ValueInput), new PropertyMetadata(default(string)));

        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        #endregion

        #region InputControl

        public static readonly DependencyProperty InputControlProperty = DependencyProperty.Register(
            nameof(InputControl), typeof(TextInput), typeof(ValueInput), new PropertyMetadata(default(TextInput)));

        public TextInput InputControl
        {
            get => (TextInput)GetValue(InputControlProperty);
            set => SetValue(InputControlProperty, value);
        }

        #endregion

        private void OnInputTypeChanged()
        {
            var inputStyleKey = "TextInputStyle";
            TextInput input = null;
            switch (InputType)
            {
                case ValueInputTypeEnum.FullName:
                    input = new TextInput()
                        {
                            MaxLength = 100,
                        };
                    input.SetValue(VirtualKeyboard.TypeProperty, VirtualKeyboardLayoutTypeEnum.Custom);
                    input.SetValue(VirtualKeyboard.CustomLayoutProviderProperty, new NameKeyboardLayoutProvider());
                    break;

                case ValueInputTypeEnum.Phone:
                    input = new PhoneNumberInput();
                    inputStyleKey = "PhoneNumberInputStyle";
                    input.SetValue(VirtualKeyboard.TypeProperty, VirtualKeyboardLayoutTypeEnum.Digits);
                    break;

                case ValueInputTypeEnum.PhoneVerificationCode:
                    input = new TextInput()
                        {
                            MaxLength = 6,
                        };
                    input.SetValue(VirtualKeyboard.TypeProperty, VirtualKeyboardLayoutTypeEnum.Digits);
                    break;

                case ValueInputTypeEnum.Address:
                    input = new TextInput()
                        {
                            MaxLength = 100,
                        };
                    input.SetValue(VirtualKeyboard.TypeProperty, VirtualKeyboardLayoutTypeEnum.Custom);
                    input.SetValue(VirtualKeyboard.CustomLayoutProviderProperty, new AddressKeyboardLayoutProvider());
                    break;

                case ValueInputTypeEnum.PromoCode:
                    input = new TextInput()
                        {
                            MaxLength = 6,
                        };
                    input.SetValue(VirtualKeyboard.TypeProperty, VirtualKeyboardLayoutTypeEnum.Digits);
                    break;
            }

            if (input != null)
            {
                input.SetBinding(VirtualKeyboardTargetBase.TextProperty, new Binding()
                    {
                        Source = this,
                        Path = new PropertyPath(nameof(Value)),
                        Mode = BindingMode.TwoWay,
                    });
                input.RelatedKeyboard = VirtualKeyboardControl;
                input.Style = ResourceHelper.GetStaticResourceFromUIThread<Style>(inputStyleKey);
                InputControl = input;
                VirtualKeyboardControl.Target = InputControl;
            }
        }

        private void ClearValue_OnClick(object sender, RoutedEventArgs e)
        {
            Value = null;
        }

        public void ShowError(string errorMessage)
        {
            ThreadHelper.EnsureUiThread();

            var flyout = new Flyout()
                {
                    Content = new InputErrorFlyoutContent(errorMessage),
                    Placement = FlyoutPlacementMode.Left,
                    FlyoutPresenterStyle = ResourceHelper.GetStaticResourceFromUIThread<Style>("TransparentFlyoutPresenterStyle"),
                };

            flyout.ShowAt(InputContainer);
        }
    }
}