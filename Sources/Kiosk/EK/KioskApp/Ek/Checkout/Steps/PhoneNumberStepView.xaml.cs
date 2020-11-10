using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using KioskApp.Controls;
using KioskApp.Search;
using KioskBrains.Common.EK.Api;
using KioskBrains.Common.Logging;

namespace KioskApp.Ek.Checkout.Steps
{
    public sealed partial class PhoneNumberStepView : UserControl
    {
        public PhoneNumberStepView()
        {
            GotoStage(PhoneNumberStepStageEnum.PhoneNumber);

            InitializeComponent();
        }

        #region Data

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(PhoneNumberStepData), typeof(PhoneNumberStepView), new PropertyMetadata(default(PhoneNumberStepData)));

        public PhoneNumberStepData Data
        {
            get => (PhoneNumberStepData)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #endregion

        private PhoneNumberStepStageEnum _currentStage;

        #region IsPhoneNumberStage

        public static readonly DependencyProperty IsPhoneNumberStageProperty = DependencyProperty.Register(
            nameof(IsPhoneNumberStage), typeof(bool), typeof(PhoneNumberStepView), new PropertyMetadata(default(bool)));

        public bool IsPhoneNumberStage
        {
            get => (bool)GetValue(IsPhoneNumberStageProperty);
            set => SetValue(IsPhoneNumberStageProperty, value);
        }

        #endregion

        #region IsVerificationCodeStage

        public static readonly DependencyProperty IsVerificationCodeStageProperty = DependencyProperty.Register(
            nameof(IsVerificationCodeStage), typeof(bool), typeof(PhoneNumberStepView), new PropertyMetadata(default(bool)));

        public bool IsVerificationCodeStage
        {
            get => (bool)GetValue(IsVerificationCodeStageProperty);
            set => SetValue(IsVerificationCodeStageProperty, value);
        }

        #endregion

        private void GotoStage(PhoneNumberStepStageEnum stage)
        {
            _currentStage = stage;

            switch (_currentStage)
            {
                case PhoneNumberStepStageEnum.PhoneNumber:
                    IsPhoneNumberStage = true;
                    IsVerificationCodeStage = false;
                    break;
                case PhoneNumberStepStageEnum.VerificationCode:
                    Data.VerificationCode = null;
#pragma warning disable 4014
                    RequestVerificationCodeAsync();
#pragma warning restore 4014
                    IsPhoneNumberStage = false;
                    IsVerificationCodeStage = true;
                    break;
            }
        }

        private string _lastReceivedVerificationCode;

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            switch (_currentStage)
            {
                case PhoneNumberStepStageEnum.PhoneNumber:
                    if (Data.PhoneNumber == null
                        || Data.PhoneNumber.Length != PhoneNumberInput.ValidPhoneNumberLength)
                    {
                        PhoneNumberValueInput.ShowError("Введите номер телефона");
                        return;
                    }

                    if (Data.PhoneNumber == Data.ConfirmedPhoneNumber)
                    {
                        WizardNextCommand?.Execute(null);
                    }
                    else
                    {
                        GotoStage(PhoneNumberStepStageEnum.VerificationCode);
                    }

                    break;

                case PhoneNumberStepStageEnum.VerificationCode:
                    if (Data.VerificationCode == null
                        || Data.VerificationCode.Length != 6)
                    {
                        VerificationCodeValueInput.ShowError("Введите 6 цифр кода");
                        return;
                    }

                    if (Data.VerificationCode != _lastReceivedVerificationCode)
                    {
                        VerificationCodeValueInput.ShowError("Код не совпадает");
                        return;
                    }

                    Data.ConfirmedPhoneNumber = Data.PhoneNumber;
                    WizardNextCommand?.Execute(null);
                    break;
            }
        }

        #region IsVerificationCodeRequesting

        public static readonly DependencyProperty IsVerificationCodeRequestingProperty = DependencyProperty.Register(
            nameof(IsVerificationCodeRequesting), typeof(bool), typeof(PhoneNumberStepView), new PropertyMetadata(default(bool)));

        public bool IsVerificationCodeRequesting
        {
            get => (bool)GetValue(IsVerificationCodeRequestingProperty);
            set => SetValue(IsVerificationCodeRequestingProperty, value);
        }

        #endregion

        private async Task RequestVerificationCodeAsync()
        {
            IsVerificationCodeSendingError = false;

            try
            {
#if DEBUG
                // DEV ONLY
                //_lastReceivedVerificationCode = "111111";
                //return;
#endif

                IsVerificationCodeRequesting = true;

                var response = await ServerApiHelper.VerifyPhoneNumberAsync(
                    new EkKioskVerifyPhoneNumberPostRequest()
                        {
                            PhoneNumber = PhoneNumberHelper.GetCleanedPhoneNumber(Data.PhoneNumber),
                        },
                    CancellationToken.None);

                _lastReceivedVerificationCode = response.VerificationCode;
                IsVerificationCodeSendingError = false;
            }
            catch (Exception ex)
            {
                IsVerificationCodeSendingError = true;
                Log.Error(LogContextEnum.Communication, nameof(RequestVerificationCodeAsync), ex);
            }
            finally
            {
                IsVerificationCodeRequesting = false;
            }
        }

        #region WizardBackCommand

        public static readonly DependencyProperty WizardBackCommandProperty = DependencyProperty.Register(
            nameof(WizardBackCommand), typeof(ICommand), typeof(PhoneNumberStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardBackCommand
        {
            get => (ICommand)GetValue(WizardBackCommandProperty);
            set => SetValue(WizardBackCommandProperty, value);
        }

        #endregion

        #region WizardNextCommand

        public static readonly DependencyProperty WizardNextCommandProperty = DependencyProperty.Register(
            nameof(WizardNextCommand), typeof(ICommand), typeof(PhoneNumberStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardNextCommand
        {
            get => (ICommand)GetValue(WizardNextCommandProperty);
            set => SetValue(WizardNextCommandProperty, value);
        }

        #endregion

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            switch (_currentStage)
            {
                case PhoneNumberStepStageEnum.PhoneNumber:
                    WizardBackCommand?.Execute(null);
                    break;
                case PhoneNumberStepStageEnum.VerificationCode:
                    GotoStage(PhoneNumberStepStageEnum.PhoneNumber);
                    break;
            }
        }

        #region IsVerificationCodeSendingError

        public static readonly DependencyProperty IsVerificationCodeSendingErrorProperty = DependencyProperty.Register(
            nameof(IsVerificationCodeSendingError), typeof(bool), typeof(PhoneNumberStepView), new PropertyMetadata(default(bool)));

        public bool IsVerificationCodeSendingError
        {
            get => (bool)GetValue(IsVerificationCodeSendingErrorProperty);
            set => SetValue(IsVerificationCodeSendingErrorProperty, value);
        }

        #endregion

        private void SendingErrorBackButton_OnClick(object sender, RoutedEventArgs e)
        {
            GotoStage(PhoneNumberStepStageEnum.PhoneNumber);
        }

        private void SendingErrorRepeatButton_OnClick(object sender, RoutedEventArgs e)
        {
            GotoStage(PhoneNumberStepStageEnum.VerificationCode);
        }
    }
}