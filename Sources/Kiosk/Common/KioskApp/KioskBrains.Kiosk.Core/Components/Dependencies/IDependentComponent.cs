using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Kiosk.Core.Components.Operations;

namespace KioskBrains.Kiosk.Core.Components.Dependencies
{
    public interface IDependentComponent
    {
        /// <summary>
        /// Initialize dependencies from contracts of other components in order to set dependency based status.
        /// Also should be used to run initial activity that is dependent on external contracts.
        /// </summary>
        Task<BasicOperationResponse> InitializeContractDependenciesAsync(EmptyRequest request, ComponentOperationContext context);
    }
}