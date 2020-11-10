using System;
using KioskBrains.Common.KioskState;

namespace KioskBrains.Kiosk.Core.Components.Statuses
{
    public class ComponentStatusCodeChangedEventArgs : EventArgs
    {
        public ComponentStatusCodeEnum OldValue { get; }

        public ComponentStatusCodeEnum NewValue { get; }

        public ComponentStatusCodeChangedEventArgs(ComponentStatusCodeEnum oldValue, ComponentStatusCodeEnum newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}