using System;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Kiosk.Core.Components
{
    public class ComponentLock : IDisposable
    {
        private readonly Action _componentLockReleaseAction;

        internal ComponentLock(Action componentLockReleaseAction)
        {
            Assure.ArgumentNotNull(componentLockReleaseAction, nameof(componentLockReleaseAction));

            _componentLockReleaseAction = componentLockReleaseAction;
        }

        public void Dispose()
        {
            _componentLockReleaseAction();
        }
    }
}