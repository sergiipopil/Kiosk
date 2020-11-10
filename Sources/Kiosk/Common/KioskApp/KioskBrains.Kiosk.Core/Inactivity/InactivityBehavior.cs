using System;

namespace KioskBrains.Kiosk.Core.Inactivity
{
    public class InactivityBehavior
    {
        public static readonly InactivityBehavior Disabled = new InactivityBehavior(DisabledDelayBeforeModalSecs);

        public const int DisabledDelayBeforeModalSecs = 0;

        public const int DefaultModalDurationSecs = 15;

        public int DelayBeforeModalSecs { get; }

        public int ModalDurationSecs { get; }

        public bool IsDisabled => DelayBeforeModalSecs == DisabledDelayBeforeModalSecs;

        public InactivityBehavior(
            int delayBeforeModalSecs,
            int modalDurationSecs = DefaultModalDurationSecs)
        {
            if (delayBeforeModalSecs < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delayBeforeModalSecs), "Must be non-negative.");
            }
            if (modalDurationSecs <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delayBeforeModalSecs), "Must be positive.");
            }

            DelayBeforeModalSecs = delayBeforeModalSecs;
            ModalDurationSecs = modalDurationSecs;
        }
    }
}