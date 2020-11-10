using System.Threading.Tasks;

namespace KioskBrains.Kiosk.Core.Components.Operations
{
    public delegate Task<TResponse> ComponentOperationImplementation<in TRequest, TResponse>(TRequest request, ComponentOperationContext context);
}