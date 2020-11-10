using System;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using KioskApp.Controls;
using KioskApp.Ek.Cart;
using KioskApp.Ek.Catalog;
using KioskApp.Ek.Catalog.AutoParts.Europe;
using KioskApp.Ek.Checkout;
using KioskApp.Ek.Checkout.Steps.Payment;
using KioskApp.Ek.Info;
using KioskApp.Ek.OrderCompletion;
using KioskApp.Ek.Receipt;
using KioskBrains.Common.Transactions;
using KioskBrains.Kiosk.Core.Components;
using KioskBrains.Kiosk.Core.Languages;
using KioskBrains.Kiosk.Core.Modals;
using KioskBrains.Kiosk.Core.Settings;
using KioskBrains.Kiosk.Helpers.Threads;
using KioskBrains.Kiosk.Helpers.Ui;

namespace KioskApp.Ek
{
    public sealed partial class EkApplicationView : UserControl
    {
        public EkApplicationView()
        {
            var kioskAppSettings = ComponentManager.Current.GetComponent<IKioskAppSettingsContract>().State;
            SupportPhone = kioskAppSettings.SupportPhone;

            EkContext.Current.GoToMainCommand = new RelayCommand(
                nameof(EkContext.GoToMainCommand),
                parameter => SetMainView());
            EkContext.Current.GoToCartCommand = new RelayCommand(
                nameof(EkContext.GoToCartCommand),
                parameter => SetCartView());
            EkContext.Current.GoToCheckoutCommand = new RelayCommand(
                nameof(EkContext.GoToCheckoutCommand),
                parameter => SetCheckoutView());
            EkContext.Current.ContinueShoppingCommand = new RelayCommand(
                nameof(EkContext.ContinueShoppingCommand),
                parameter => OnContinueShoppingCommand());
            EkContext.Current.CompleteOrderCommand = new RelayCommand(
                nameof(EkContext.CompleteOrderCommand),
                parameter => OnCompleteOrderCommand());
            EkContext.Current.SetViewCommand = new RelayCommand(
                nameof(EkContext.CompleteOrderCommand),
                parameter => OnSetViewCommand(parameter as UserControl));

            InitializeComponent();
        }

        private void EkApplicationView_OnLoaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged += OnLanguageChanged;

            UpdateLabels();

            ResetForNewUser(TransactionStatusEnum.Unspecified);
        }

        private void EkApplicationView_OnUnloaded(object sender, RoutedEventArgs e)
        {
            LanguageManager.Current.LanguageChanged -= OnLanguageChanged;
        }

        public EkContext Context => EkContext.Current;

        #region Globalization

        private async void LanguageSelector_OnLanguageSelected(object sender, LanguageSelectedEventArgs e)
        {
            await LanguageManager.Current.SetAppLanguageAsync(e.Language);

            EkContext.EkProcess?.OnLanguageSelected(e.Language);
        }

        private void UpdateLabels()
        {
            ThreadHelper.EnsureUiThread();

            MainLinkLabel = LanguageManager.Current.GetLocalizedString("Main_MainLink");
            NextUserLabel = LanguageManager.Current.GetLocalizedString("Main_NextUser");
        }

        private void OnLanguageChanged(object sender, Language e)
        {
            ThreadHelper.RunInUiThreadAsync(UpdateLabels);
        }

        #endregion

        #region IsLoading

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading), typeof(bool), typeof(EkApplicationView), new PropertyMetadata(default(bool)));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        #endregion

        #region SupportPhone

        public static readonly DependencyProperty SupportPhoneProperty = DependencyProperty.Register(
            nameof(SupportPhone), typeof(string), typeof(EkApplicationView), new PropertyMetadata(default(string)));

        public string SupportPhone
        {
            get => (string)GetValue(SupportPhoneProperty);
            set => SetValue(SupportPhoneProperty, value);
        }

        #endregion

        #region MainLinkLabel

        public static readonly DependencyProperty MainLinkLabelProperty = DependencyProperty.Register(
            nameof(MainLinkLabel), typeof(string), typeof(EkApplicationView), new PropertyMetadata(default(string)));

        public string MainLinkLabel
        {
            get => (string)GetValue(MainLinkLabelProperty);
            set => SetValue(MainLinkLabelProperty, value);
        }

        #endregion

        #region NextUserLabel

        public static readonly DependencyProperty NextUserLabelProperty = DependencyProperty.Register(
            nameof(NextUserLabel), typeof(string), typeof(EkApplicationView), new PropertyMetadata(default(string)));

        public string NextUserLabel
        {
            get => (string)GetValue(NextUserLabelProperty);
            set => SetValue(NextUserLabelProperty, value);
        }

        #endregion

        #region CurrentView

        public static readonly DependencyProperty CurrentViewProperty = DependencyProperty.Register(
            nameof(CurrentView), typeof(UserControl), typeof(EkApplicationView), new PropertyMetadata(default(UserControl)));

        public UserControl CurrentView
        {
            get => (UserControl)GetValue(CurrentViewProperty);
            set => SetValue(CurrentViewProperty, value);
        }

        #endregion

        #region State

        public EkContext EkContext => EkContext.Current;

        private void InitNewEkProcess()
        {
            EkContext.EkProcess?.End();
            EkContext.EkProcess = new EkProcess(ResetForNewUser);
        }

        private async void ResetForNewUser(TransactionStatusEnum completedProcessTransactionStatus)
        {
            // run reset in UI thread
            await ThreadHelper.RunInUiThreadAsync(async () =>
                {
                    IsLoading = true;

                    // reset caches
                    _cachedViewForShoppingContinue = null;

                    // close all modals (like info modals)
                    await ModalManager.Current.ClearModals();

                    // complete transaction if started
                    EkContext.EkProcess?.CompleteTransaction(completedProcessTransactionStatus);

                    await ResetLanguageAsync();

                    InitNewEkProcess();

                    // main view if all other cases
                    SetMainView(onResetForNewUser: true);

#if DEBUG
                    // DEV ONLY: go to checkout
                    //EkContext.EkProcess?.Cart.AddToCartCommand.Execute(
                    //    new Search.Product(
                    //        new EkProduct()
                    //            {
                    //                Key = "AllegroPl_1234",
                    //                Source = EkProductSourceEnum.AllegroPl,
                    //                Name = new MultiLanguageString()
                    //                    {
                    //                        [KioskBrains.Common.Constants.Languages.RussianCode] = "Test Product",
                    //                    },
                    //                Price = 12345,
                    //            }));
                    //EkContext.GoToCheckoutCommand.Execute(null);
                    //CompleteOrderCommand.Execute(null);
#endif

                    IsLoading = false;
                });
        }

        private async Task ResetLanguageAsync()
        {
            await LanguageManager.Current.SetAppLanguageAsync(LanguageManager.Current.KioskLanguages[0]);
        }

        #endregion

        #region ShowContinueShoppingWidget

        public static readonly DependencyProperty ShowContinueShoppingWidgetProperty = DependencyProperty.Register(
            nameof(ShowContinueShoppingWidget), typeof(bool), typeof(EkApplicationView), new PropertyMetadata(default(bool)));

        public bool ShowContinueShoppingWidget
        {
            get => (bool)GetValue(ShowContinueShoppingWidgetProperty);
            set => SetValue(ShowContinueShoppingWidgetProperty, value);
        }

        #endregion

        #region Navigation

        private void SetMainView(bool onResetForNewUser = false)
        {
            // just in case
            EkContext.Current.HideMenuCounter.Reset();

            SetView(new EuropeMainView(), onResetForNewUser);
        }

        private UserControl _cachedViewForShoppingContinue;

        private void SetCartView()
        {
            SetView(new CartView());
        }

        private void SetCheckoutView()
        {
            SetView(new CheckoutView());
        }

        private async void OnCompleteOrderCommand()
        {
            IsLoading = true;
            var ekProcess = EkContext.EkProcess;

            var receiptData = await ekProcess.GenerateReceiptDataAsync();

            // print receipt in parallel
#pragma warning disable 4014
            Printer.PrintReceiptAsync(receiptData);
#pragma warning restore 4014

            IsLoading = false;

            SetOrderCompletionView(ekProcess.SelectedPaymentMethodInfo);

            ekProcess.CompleteTransaction(TransactionStatusEnum.Completed);
        }

        private void SetOrderCompletionView(PaymentMethodInfo selectedPaymentMethodInfo)
        {
            SetView(new OrderCompletionView()
                {
                    NextUserCommand = EkContext.ContinueShoppingCommand,
                    SelectedPaymentMethodInfo = selectedPaymentMethodInfo,
                });
        }

        private void OnContinueShoppingCommand()
        {
            if (_cachedViewForShoppingContinue != null)
            {
                SetView(_cachedViewForShoppingContinue);
                _cachedViewForShoppingContinue = null;
            }
            else
            {
                SetMainView();
            }
        }

        private void OnSetViewCommand(UserControl view)
        {
            if (view != null)
            {
                SetView(view);
            }
        }

        private bool IsCheckoutProcessView(UserControl view)
        {
            return view is CartView
                   || view is CheckoutView;
        }

        private void SetView(UserControl view, bool onResetForNewUser = false)
        {
            ThreadHelper.EnsureUiThread();

            var isCheckoutProcessView = IsCheckoutProcessView(view);
            if (isCheckoutProcessView
                && !IsCheckoutProcessView(CurrentView))
            {
                _cachedViewForShoppingContinue = CurrentView;
            }
            else if (CurrentView is OrderCompletionView & !onResetForNewUser)
            {
                // reset on exit from order completion view (if it's not exit by reset process triggered by timeout or 'new user' button)
                ResetForNewUser(TransactionStatusEnum.Unspecified);
            }

            CurrentView = view;

            ShowContinueShoppingWidget = isCheckoutProcessView
                                         || view is OrderCompletionView;

            EkContext.EkProcess?.OnViewChanged(view.GetType().Name, view is EuropeMainView);
        }

        private void Logo_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SetMainView();
        }

        private void Home_OnMenuPointClicked(object sender, MenuPointClickedEventArgs e)
        {
            SetMainView();
        }

        private void NextUser_OnClick(object sender, RoutedEventArgs e)
        {
            ResetForNewUser(TransactionStatusEnum.CancelledByUser);
        }

        private void Warranty_OnMenuPointClicked(object sender, MenuPointClickedEventArgs e)
        {
            InfoModalHelper.OpenInfoModal(InfoModalTypeEnum.Warranty);
        }

        private void Return_OnMenuPointClicked(object sender, MenuPointClickedEventArgs e)
        {
            InfoModalHelper.OpenInfoModal(InfoModalTypeEnum.Return);
        }

        private void Soon_OnMenuPointClicked(object sender, MenuPointClickedEventArgs e)
        {
            SoonFlyout.ShowAt((FrameworkElement)sender);
        }

        #endregion
    }
}