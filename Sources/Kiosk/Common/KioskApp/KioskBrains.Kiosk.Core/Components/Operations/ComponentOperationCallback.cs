using System;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Components.Operations
{
    /// <summary>
    /// Component contract operation callback.
    /// Can't be inherited - should be used directly (convention).
    /// </summary>
    public sealed class ComponentOperationCallback<TRequest, TResponse>
        where TRequest : class
        where TResponse : IComponentOperationResponse, new()
    {
        private readonly string _callbackName;

        private readonly ComponentOperationCallbackImplementation<TRequest, TResponse> _implementation;

        public ComponentOperationCallback(
            string callbackName,
            ComponentOperationCallbackImplementation<TRequest, TResponse> implementation
        )
        {
            Assure.ArgumentNotNull(callbackName, nameof(callbackName));
            Assure.ArgumentNotNull(implementation, nameof(implementation));

            _callbackName = callbackName;
            _implementation = implementation;
        }

        public Task<TResponse> InvokeAsync(TRequest request, ComponentOperationContext baseOperationContext, bool throwOnError = false)
        {
            return ThreadHelper.RunInNewThreadAsync(async () =>
            {
                try
                {
                    Assure.ArgumentNotNull(request, nameof(request));

                    var response = await _implementation(request);

                    if (response == null)
                    {
                        throw new InvalidFlowStateException("Operation callback returned 'null'.");
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    baseOperationContext.Log.Error(LogContextEnum.Component, "Operation callback exception.", ex, callerName: $"'{_callbackName}' callback");

                    if (throwOnError)
                    {
                        throw new ComponentOperationException($"'{_callbackName}' callback error '{ex.Message}'.", ex);
                    }

                    return new TResponse()
                    {
                        Status = ComponentOperationStatusEnum.Error,
                    };
                }
            });
        }
    }
}