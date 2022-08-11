using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Checkout.Steps.Delivery
{
    public sealed partial class CourierDataInputStageView : UserControl
    {
        public CourierDataInputStageView()
        {
            InitializeComponent();
        }

        #region Data

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(DeliveryInfoStepData), typeof(CourierDataInputStageView), new PropertyMetadata(default(DeliveryInfoStepData), DataPropertyChangedCallback));

        private static void DataPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CourierDataInputStageView)d).OnDataChanged();
        }

        public DeliveryInfoStepData Data
        {
            get => (DeliveryInfoStepData)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #endregion

        private void OnDataChanged()
        {
            if (!string.IsNullOrEmpty(Data.Address.City))
            {
                GotoInput(CourierDataInputEnum.AddressLine1);
            }
            else
            {
                GotoInput(CourierDataInputEnum.City);
            }
        }

        #region BackCommand

        public static readonly DependencyProperty BackCommandProperty = DependencyProperty.Register(
            nameof(BackCommand), typeof(ICommand), typeof(CourierDataInputStageView), new PropertyMetadata(default(ICommand)));

        public ICommand BackCommand
        {
            get => (ICommand)GetValue(BackCommandProperty);
            set => SetValue(BackCommandProperty, value);
        }

        #endregion

        #region NextCommand

        public static readonly DependencyProperty NextCommandProperty = DependencyProperty.Register(
            nameof(NextCommand), typeof(ICommand), typeof(CourierDataInputStageView), new PropertyMetadata(default(ICommand)));

        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }

        #endregion

        #region ShowCityInput

        public static readonly DependencyProperty ShowCityInputProperty = DependencyProperty.Register(
            nameof(ShowCityInput), typeof(bool), typeof(CourierDataInputStageView), new PropertyMetadata(default(bool)));

        public bool ShowCityInput
        {
            get => (bool)GetValue(ShowCityInputProperty);
            set => SetValue(ShowCityInputProperty, value);
        }

        #endregion

        #region ShowAddressLine1Input

        public static readonly DependencyProperty ShowAddressLine1InputProperty = DependencyProperty.Register(
            nameof(ShowAddressLine1Input), typeof(bool), typeof(CourierDataInputStageView), new PropertyMetadata(default(bool)));

        public bool ShowAddressLine1Input
        {
            get => (bool)GetValue(ShowAddressLine1InputProperty);
            set => SetValue(ShowAddressLine1InputProperty, value);
        }

        #endregion

        private CourierDataInputEnum _currentInput;

        private void GotoInput(CourierDataInputEnum input)
        {
            _currentInput = input;

            ShowCityInput = false;
            ShowAddressLine1Input = false;

            switch (input)
            {
                case CourierDataInputEnum.City:
                    ShowCityInput = true;
                    break;
                case CourierDataInputEnum.AddressLine1:
                    ShowAddressLine1Input = true;
                    break;
            }
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            switch (_currentInput)
            {
                case CourierDataInputEnum.City:
                    BackCommand?.Execute(null);
                    break;
                case CourierDataInputEnum.AddressLine1:
                    GotoInput(CourierDataInputEnum.City);
                    break;
            }
        }

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            switch (_currentInput)
            {
                case CourierDataInputEnum.City:
                    if (string.IsNullOrWhiteSpace(Data.Address.City))
                    {
                        CityValueInput.ShowError("Введіть місто");
                        return;
                    }

                    GotoInput(CourierDataInputEnum.AddressLine1);
                    break;

                case CourierDataInputEnum.AddressLine1:
                    if (string.IsNullOrWhiteSpace(Data.Address.AddressLine1))
                    {
                        AddressLine1ValueInput.ShowError("Введіть місто");
                        return;
                    }

                    NextCommand?.Execute(null);
                    break;
            }
        }
    }
}