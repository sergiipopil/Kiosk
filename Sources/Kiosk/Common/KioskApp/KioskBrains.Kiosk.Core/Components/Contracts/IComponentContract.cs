using System.ComponentModel;

namespace KioskBrains.Kiosk.Core.Components.Contracts
{
    public interface IComponentContract : INotifyPropertyChanged
    {
        /// <summary>
        /// Inherited from ComponentBase.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Inherited from ComponentBase.
        /// </summary>
        string ComponentRole { get; }

        /// <summary>
        /// Inherited from ComponentBase.
        /// </summary>
        ComponentLock Lock();

        /// <summary>
        /// Inherited from ComponentBase. Only for testing purposes.
        /// </summary>
        object SpecificMonitorableState { get; }
    }
}