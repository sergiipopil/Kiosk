using System;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Controls;
using KioskApp.Ek.Checkout.Steps;
using KioskApp.Ek.Checkout.Steps.Delivery;
using KioskApp.Ek.Checkout.Steps.Payment;
using KioskBrains.Common.EK.Transactions;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Ek.Checkout
{
    public sealed partial class CheckoutView : UserControl
    {
        public CheckoutView()
        {
            
            WizardBackCommand = new RelayCommand(
                nameof(WizardBackCommand),
                x => OnWizardBackCommand());

            WizardNextCommand = new RelayCommand(
                nameof(WizardNextCommand),
                x => OnWizardNextCommand());

            WizardSteps = new[]
                {
                    new CheckoutWizardStep(CheckoutStepEnum.PersonalInfo),
                    new CheckoutWizardStep(CheckoutStepEnum.PhoneNumber),
                    new CheckoutWizardStep(CheckoutStepEnum.DeliveryInfo),
                    new CheckoutWizardStep(CheckoutStepEnum.OrderConfirmation),
                    new CheckoutWizardStep(CheckoutStepEnum.PaymentInfo),
                    new CheckoutWizardStep(CheckoutStepEnum.ReceiptPrinting),
                };

            InitializeComponent();

#if DEBUG
            // DEV ONLY
            //GotoStep(CheckoutStepEnum.PaymentInfo);
            //return;
#endif

            GotoNextStep();            
            txtCartProducts.Text = string.Join("\n", EkContext.EkProcess.Cart.Products.Select(x => x.Product.Name));
        }

        public EkContext EkContext => EkContext.Current;

        private void CheckoutView_OnLoaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged += OnLanguageChanged;

            UpdateLabels();
        }

        private void CheckoutView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged -= OnLanguageChanged;
        }

        #region Globalization

        private void UpdateLabels()
        {
            ThreadHelper.EnsureUiThread();

            foreach (var wizardItem in WizardSteps)
            {
                switch (wizardItem.Step)
                {
                    case CheckoutStepEnum.PersonalInfo:
                        wizardItem.Name = "Имя и фамилия";
                        break;
                    case CheckoutStepEnum.PhoneNumber:
                        wizardItem.Name = "Номер телефона";
                        break;
                    case CheckoutStepEnum.DeliveryInfo:
                        wizardItem.Name = "Способ доставки";
                        break;
                    case CheckoutStepEnum.OrderConfirmation:
                        wizardItem.Name = "Подтверждение";
                        break;
                    case CheckoutStepEnum.PaymentInfo:
                        wizardItem.Name = "Способ оплаты";
                        break;
                    case CheckoutStepEnum.ReceiptPrinting:
                        wizardItem.Name = "Заказ оформлен!";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void OnLanguageChanged(object sender, Language e)
        {
            ThreadHelper.RunInUiThreadAsync(UpdateLabels);
        }

        #endregion

        public CheckoutWizardStep[] WizardSteps { get; }

        #region Navigation

        private void GotoNextStep()
        {
            var wizardStep = WizardSteps
                .Where(x => x.State == WizardStepStateEnum.Next)
                .FirstOrDefault();
            GotoStep(wizardStep?.Step ?? CheckoutStepEnum.PersonalInfo);
        }

        private CheckoutStepEnum _currentStep;

        private void GotoStep(CheckoutStepEnum step)
        {
            foreach (var wizardStep in WizardSteps)
            {
                wizardStep.IsActive = wizardStep.Step == step;
            }

            _currentStep = step;

            switch (step)
            {
                case CheckoutStepEnum.PersonalInfo:
                    StepView = new PersonalInfoStepView()
                        {
                            Data = _personalInfoStepData,
                            WizardBackCommand = WizardBackCommand,
                            WizardNextCommand = WizardNextCommand,
                        };
                    break;
                case CheckoutStepEnum.PhoneNumber:
                    StepView = new PhoneNumberStepView()
                        {
                            Data = _phoneNumberStepData,
                            WizardBackCommand = WizardBackCommand,
                            WizardNextCommand = WizardNextCommand,
                        };
                    break;
                case CheckoutStepEnum.DeliveryInfo:
                    StepView = new DeliveryInfoStepView()
                        {
                            Data = _deliveryInfoStepData,
                            WizardBackCommand = WizardBackCommand,
                            WizardNextCommand = WizardNextCommand,
                        };
                    break;
                case CheckoutStepEnum.OrderConfirmation:
                    StepView = new OrderConfirmationStepView()
                        {
                            WizardBackCommand = WizardBackCommand,
                            WizardNextCommand = WizardNextCommand,
                        };
                    break;
                case CheckoutStepEnum.PaymentInfo:
                    StepView = new PaymentInfoStepView()
                        {
                            Data = _paymentInfoStepData,
                            WizardBackCommand = WizardBackCommand,
                            WizardNextCommand = WizardNextCommand,
                        };
                    break;
                default:
                    StepView = null;
                    break;
            }
        }

        public ICommand WizardBackCommand { get; }

        public ICommand WizardNextCommand { get; }

        private void OnWizardBackCommand()
        {
            switch (_currentStep)
            {
                case CheckoutStepEnum.PersonalInfo:
                    EkContext.GoToCartCommand.Execute(null);
                    break;

                case CheckoutStepEnum.PhoneNumber:
                    GotoStep(CheckoutStepEnum.PersonalInfo);
                    break;

                case CheckoutStepEnum.DeliveryInfo:
                    GotoStep(CheckoutStepEnum.PhoneNumber);
                    break;

                case CheckoutStepEnum.OrderConfirmation:
                    GotoStep(CheckoutStepEnum.DeliveryInfo);
                    break;

                case CheckoutStepEnum.PaymentInfo:
                    GotoStep(CheckoutStepEnum.OrderConfirmation);
                    break;
            }
        }

        private void SetWizardStepValue(CheckoutStepEnum step, string value)
        {
            var wizardStep = WizardSteps
                .Where(x => x.Step == step)
                .FirstOrDefault();
            if (wizardStep != null)
            {
                wizardStep.Value = value;
            }
        }

        private PersonalInfoStepData _personalInfoStepData { get; } = new PersonalInfoStepData();

        private PhoneNumberStepData _phoneNumberStepData { get; } = new PhoneNumberStepData();

        private DeliveryInfoStepData _deliveryInfoStepData { get; } = new DeliveryInfoStepData();

        private PaymentInfoStepData _paymentInfoStepData { get; } = new PaymentInfoStepData();

        private void OnWizardNextCommand()
        {
            switch (_currentStep)
            {
                case CheckoutStepEnum.PersonalInfo:
                    SetWizardStepValue(CheckoutStepEnum.PersonalInfo, _personalInfoStepData.FullName);
                    GotoStep(CheckoutStepEnum.PhoneNumber);
                    break;

                case CheckoutStepEnum.PhoneNumber:
                    SetWizardStepValue(CheckoutStepEnum.PhoneNumber, _phoneNumberStepData.PhoneNumber);
                    EkContext.EkProcess.OnCustomerInfoInput(new EkCustomerInfo()
                        {
                            FullName = _personalInfoStepData.FullName,
                            Phone = PhoneNumberHelper.GetCleanedPhoneNumber(_phoneNumberStepData.PhoneNumber),
                        });
                    GotoStep(CheckoutStepEnum.DeliveryInfo);
                    break;

                case CheckoutStepEnum.DeliveryInfo:
                    var deliveryValueBuilder = new StringBuilder();
                    switch (_deliveryInfoStepData.Type)
                    {
                        case EkDeliveryTypeEnum.DeliveryServiceStore:
                            deliveryValueBuilder.Append("Самовывоз, Новая Почта");
                            break;
                        case EkDeliveryTypeEnum.Courier:
                            deliveryValueBuilder.Append("Курьер");
                            break;
                    }

                    deliveryValueBuilder.Append($", {_deliveryInfoStepData.Address.City}");
                    deliveryValueBuilder.Append($", {_deliveryInfoStepData.Address.AddressLine1}");
                    SetWizardStepValue(CheckoutStepEnum.DeliveryInfo, deliveryValueBuilder.ToString());

                    var ekDeliveryInfo = new EkDeliveryInfo
                        {
                            Type = _deliveryInfoStepData.Type ?? EkDeliveryTypeEnum.Courier,
                            DeliveryService = _deliveryInfoStepData.DeliveryService,
                            StoreId = _deliveryInfoStepData.StoreId,
                            Address = new EkTransactionAddress()
                                {
                                    City = _deliveryInfoStepData.Address.City,
                                    AddressLine1 = _deliveryInfoStepData.Address.AddressLine1,
                                }
                        };
                    EkContext.EkProcess.OnDeliveryInfoInput(ekDeliveryInfo);

                    GotoStep(CheckoutStepEnum.OrderConfirmation);
                    break;

                case CheckoutStepEnum.OrderConfirmation:
                    SetWizardStepValue(CheckoutStepEnum.OrderConfirmation, "Подтверждено");
                    GotoStep(CheckoutStepEnum.PaymentInfo);
                    break;

                case CheckoutStepEnum.PaymentInfo:
                    EkContext.EkProcess.SelectedPaymentMethodInfo = _paymentInfoStepData.PaymentMethodInfo;
                    EkContext.CompleteOrderCommand.Execute(null);
                    break;
            }
        }

        #endregion

        #region StepView

        public static readonly DependencyProperty StepViewProperty = DependencyProperty.Register(
            nameof(StepView), typeof(UserControl), typeof(CheckoutView), new PropertyMetadata(default(UserControl)));

        public UserControl StepView
        {
            get => (UserControl)GetValue(StepViewProperty);
            set => SetValue(StepViewProperty, value);
        }

        #endregion
    }
}