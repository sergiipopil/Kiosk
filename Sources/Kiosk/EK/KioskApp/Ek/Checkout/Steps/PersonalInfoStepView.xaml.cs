using System;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace KioskApp.Ek.Checkout.Steps
{
    public sealed partial class PersonalInfoStepView : UserControl
    {
        public PersonalInfoStepView()
        {
            InitializeComponent();
        }

        #region Data

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            nameof(Data), typeof(PersonalInfoStepData), typeof(PersonalInfoStepView), new PropertyMetadata(default(PersonalInfoStepData)));

        public PersonalInfoStepData Data
        {
            get => (PersonalInfoStepData)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        #endregion

        private void NextButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Data.FullName))
            {
                ValueInput.ShowError("Введите имя и фамилию");
                return;
            }

            // verify valid format
            var fullNameParts = Data.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (fullNameParts.Length < 2)
            {
                ValueInput.ShowError("Должно состоять из 2-ух слов (имя и фамилия)");
                return;
            }

            WizardNextCommand?.Execute(null);
        }

        #region WizardBackCommand

        public static readonly DependencyProperty WizardBackCommandProperty = DependencyProperty.Register(
            nameof(WizardBackCommand), typeof(ICommand), typeof(PersonalInfoStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardBackCommand
        {
            get => (ICommand)GetValue(WizardBackCommandProperty);
            set => SetValue(WizardBackCommandProperty, value);
        }

        #endregion

        #region WizardNextCommand

        public static readonly DependencyProperty WizardNextCommandProperty = DependencyProperty.Register(
            nameof(WizardNextCommand), typeof(ICommand), typeof(PersonalInfoStepView), new PropertyMetadata(default(ICommand)));

        public ICommand WizardNextCommand
        {
            get => (ICommand)GetValue(WizardNextCommandProperty);
            set => SetValue(WizardNextCommandProperty, value);
        }

        #endregion
    }
}