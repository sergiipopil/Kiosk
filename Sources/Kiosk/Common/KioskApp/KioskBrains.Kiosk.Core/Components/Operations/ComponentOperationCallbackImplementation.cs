using System.Threading.Tasks;

namespace KioskBrains.Kiosk.Core.Components.Operations
{
    public delegate Task<TResponse> ComponentOperationCallbackImplementation<in TRequest, TResponse>(TRequest request);
}