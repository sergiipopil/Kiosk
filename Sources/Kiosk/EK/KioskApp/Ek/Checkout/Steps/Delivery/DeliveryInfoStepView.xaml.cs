using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskBrains.Common.EK.Transactions;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Ek.Checkout.Steps.Delivery
{
    public sealed partial class DeliveryInfoStepView : UserControl
    {
        public DeliveryInfoStepView()
        {
            BackCommand = new RelayCommand(
                nameof(BackCommand),
                parameter => OnBack());

            NextCommand = new RelayCommand(
                nameof(NextCommand),
                parameter => OnNext());

            InitializeComponent();
        }

        #region Data

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(DeliveryInfoStepData), typeof(DeliveryInfoStepView), new PropertyMetadata(default(DeliveryInfoStepData), DataPropertyChangedCallback));

        private static void DataPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DeliveryInfoStepView)d).OnDataChanged();
        }

        public DeliveryInfoStepData Data
        {
            get => (DeliveryInfoStepData)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #endregion

        private void OnDataChanged()
        {
            switch (Data.Type)
            {
                case EkDeliveryTypeEnum.DeliveryServiceStore:
                    GotoStage(DeliveryInfoStepStageEnum.NovaPoshtaUkraineBranchSelection);
                    break;
                case EkDeliveryTypeEnum.Courier:
                    GotoStage(DeliveryInfoStepStageEnum.CourierDataInput);
                    break;
                default:
                    GotoStage(DeliveryInfoStepStageEnum.DeliveryTypeSelection);
                    break;
            }
        }

        private DeliveryInfoStepStageEnum _currentStage;

        private void GotoStage(DeliveryInfoStepStageEnum stage)
        {
            _currentStage = stage;

            switch (_currentStage)
            {
                case DeliveryInfoStepStageEnum.DeliveryTypeSelection:
                    var deliveryTypeSelectionStageView = new DeliveryTypeSelectionStageView()
                        {
                            BackCommand = BackCommand,
                        };
                    deliveryTypeSelectionStageView.DeliveryTypeSelected += (sender, deliveryType) =>
                        {
                            switch (deliveryType)
                            {
                                case EkDeliveryTypeEnum.DeliveryServiceStore:
                                    GotoStage(DeliveryInfoStepStageEnum.NovaPoshtaUkraineBranchSelection);
                                    break;
                                case EkDeliveryTypeEnum.Courier:
                                    GotoStage(DeliveryInfoStepStageEnum.CourierDataInput);
                                    break;
                            }
                        };
                    StageView = deliveryTypeSelectionStageView;
                    break;
                case DeliveryInfoStepStageEnum.CourierDataInput:
                    ResetDataIfTypeChanged(EkDeliveryTypeEnum.Courier);
                    StageView = new CourierDataInputStageView()
                        {
                            Data = Data,
                            BackCommand = BackCommand,
                            NextCommand = NextCommand,
                        };
                    break;
                case DeliveryInfoStepStageEnum.NovaPoshtaUkraineBranchSelection:
                    ResetDataIfTypeChanged(EkDeliveryTypeEnum.DeliveryServiceStore);
                    Data.DeliveryService = EkDeliveryServiceEnum.NovaPoshtaUkraine;
                    var novaPoshtaUkraineBranchSelectionStageView = new NovaPoshtaUkraineBranchSelectionStageView()
                        {
                            BackCommand = BackCommand,
                        };
                    novaPoshtaUkraineBranchSelectionStageView.BranchSelected += (sender, branch) =>
                        {
                            if (branch == null)
                            {
                                return;
                            }

                            Data.StoreId = branch.StoreId;
                            Data.Address.City = branch.City;
                            Data.Address.AddressLine1 = branch.AddressLine1;

                            OnNext();
                        };
                    StageView = novaPoshtaUkraineBranchSelectionStageView;
                    break;
            }
        }

        private void ResetDataIfTypeChanged(EkDeliveryTypeEnum newType)
        {
            if (Data.Type != newType)
            {
                Data.Type = newType;
                Data.DeliveryService = null;
                Data.StoreId = null;
                Data.Address.City = null;
                Data.Address.AddressLine1 = null;
            }
        }

        #region StageView

        public static readonly DependencyProperty StageViewProperty = DependencyProperty.Register(
            nameof(StageView), typeof(UserControl), typeof(DeliveryInfoStepView), new PropertyMetadata(default(UserControl)));

        public UserControl StageView
        {
            get => (UserControl)GetValue(StageViewProperty);
            set => SetValue(StageViewProperty, value);
        }

        #endregion

        #region WizardBackCommand

        public static readonly DependencyProperty WizardBackCommandProperty = DependencyProperty.Register(
            nameof(WizardBackCommand), typeof(ICommand), typeof(DeliveryInfoStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardBackCommand
        {
            get => (ICommand)GetValue(WizardBackCommandProperty);
            set => SetValue(WizardBackCommandProperty, value);
        }

        #endregion

        #region WizardNextCommand

        public static readonly DependencyProperty WizardNextCommandProperty = DependencyProperty.Register(
            nameof(WizardNextCommand), typeof(ICommand), typeof(DeliveryInfoStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardNextCommand
        {
            get => (ICommand)GetValue(WizardNextCommandProperty);
            set => SetValue(WizardNextCommandProperty, value);
        }

        #endregion

        public ICommand BackCommand { get; set; }

        public ICommand NextCommand { get; set; }

        private void OnBack()
        {
            switch (_currentStage)
            {
                case DeliveryInfoStepStageEnum.DeliveryTypeSelection:
                    WizardBackCommand?.Execute(null);
                    break;
                case DeliveryInfoStepStageEnum.CourierDataInput:
                    GotoStage(DeliveryInfoStepStageEnum.DeliveryTypeSelection);
                    break;
                case DeliveryInfoStepStageEnum.NovaPoshtaUkraineBranchSelection:
                    GotoStage(DeliveryInfoStepStageEnum.DeliveryTypeSelection);
                    break;
            }
        }

        private void OnNext()
        {
            WizardNextCommand?.Execute(null);
        }
    }
}