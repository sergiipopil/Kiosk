using System;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Modals;
using KioskBrains.Kiosk.Helpers.Ui.Binding;

namespace KioskBrains.Kiosk.Core.Inactivity
{
    public class InactivityConfirmationModalModel : UiBindableObject
    {
        public ModalContext ModalContext { get; }

        #region CountdownSeconds

        private int _countdownSeconds;

        public int CountdownSeconds
        {
            get { return _countdownSeconds; }
            set { base.SetProperty(ref _countdownSeconds, value); }
        }

        #endregion

        private readonly Action _onCountdownRunOut;

        public InactivityConfirmationModalModel(ModalContext modalContext, int countdownSeconds, Action onCountdownRunOut)
        {
            Assure.ArgumentNotNull(modalContext, nameof(modalContext));
            Assure.ArgumentNotNull(onCountdownRunOut, nameof(onCountdownRunOut));

            ModalContext = modalContext;
            _countdownSeconds = countdownSeconds;
            _onCountdownRunOut = onCountdownRunOut;
        }

        private bool _ignoreCountdown;

        internal void IgnoreCountdown()
        {
            _ignoreCountdown = true;
        }
        
        public void OnCountdownRunOut()
        {
            if (_ignoreCountdown)
            {
                return;
            }

            _onCountdownRunOut();
        }
    }
}