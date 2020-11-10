using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Core.Components.States;
using KioskBrains.Kiosk.Core.Components.Statuses;

namespace KioskBrains.Kiosk.Core.Components.Contracts
{
    internal class ComponentContractInfo
    {
        public ComponentContractInfo(ComponentBase component, BindableComponentStatus contractStatus, ComponentState contractState)
        {
            Assure.ArgumentNotNull(component, nameof(component));
            Assure.ArgumentNotNull(contractStatus, nameof(contractStatus));

            Component = component;
            ContractStatus = contractStatus;
            ContractState = contractState;
        }

        public ComponentBase Component { get; }

        public BindableComponentStatus ContractStatus { get; }

        public ComponentState ContractState { get; }
    }
}