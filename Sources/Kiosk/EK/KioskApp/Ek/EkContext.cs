using System.Windows.Input;
using KioskApp.Helpers;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskApp.Ek
{
    /// <summary>
    /// Container of common aspects that are passed through between views.
    /// </summary>
    public class EkContext : UiBindableObject
    {
        #region Singleton

        public static EkContext Current { get; } = new EkContext();

        private EkContext()
        {
        }

        #endregion

        #region EkProcess

        private EkProcess _ekProcess;

        public EkProcess EkProcess
        {
            get => _ekProcess;
            set => SetProperty(ref _ekProcess, value);
        }

        #endregion

        #region GotoMainCommand

        private ICommand _GoToMainCommand;

        public ICommand GoToMainCommand
        {
            get => _GoToMainCommand;
            set => SetProperty(ref _GoToMainCommand, value);
        }

        #endregion

        #region GoToCartCommand

        private ICommand _GoToCartCommand;

        public ICommand GoToCartCommand
        {
            get => _GoToCartCommand;
            set => SetProperty(ref _GoToCartCommand, value);
        }

        #endregion

        #region ContinueShoppingCommand

        private ICommand _ContinueShoppingCommand;

        public ICommand ContinueShoppingCommand
        {
            get => _ContinueShoppingCommand;
            set => SetProperty(ref _ContinueShoppingCommand, value);
        }

        #endregion

        #region GoToCheckoutCommand

        private ICommand _GoToCheckoutCommand;

        public ICommand GoToCheckoutCommand
        {
            get => _GoToCheckoutCommand;
            set => SetProperty(ref _GoToCheckoutCommand, value);
        }

        #endregion

        #region CompleteOrderCommand

        private ICommand _CompleteOrderCommand;

        public ICommand CompleteOrderCommand
        {
            get => _CompleteOrderCommand;
            set => SetProperty(ref _CompleteOrderCommand, value);
        }

        #endregion

        #region SetViewCommand

        private ICommand _SetViewCommand;

        public ICommand SetViewCommand
        {
            get => _SetViewCommand;
            set => SetProperty(ref _SetViewCommand, value);
        }

        #endregion

        public BooleanCounter HideMenuCounter { get; } = new BooleanCounter();
    }
}