using System;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Common.Logging;
using KioskBrains.Kiosk.Helpers.Threads;

namespace KioskBrains.Kiosk.Core.Components.Operations
{
    /// <summary>
    /// Component contract operation.
    /// Can't be inherited - should be used directly (convention).
    /// </summary>
    public sealed class ComponentOperation<TRequest, TResponse>
        where TRequest : class
        where TResponse : IComponentOperationResponse, new()
    {
        private readonly ComponentBase _component;

        public string OperationName { get; }

        private readonly ComponentOperationImplementation<TRequest, TResponse> _implementation;

        public ComponentOperation(
            ComponentBase component,
            string operationName,
            ComponentOperationImplementation<TRequest, TResponse> implementation
        )
        {
            Assure.ArgumentNotNull(component, nameof(component));
            Assure.ArgumentNotNull(operationName, nameof(operationName));
            Assure.ArgumentNotNull(implementation, nameof(implementation));

            _component = component;
            OperationName = operationName;
            _implementation = implementation;
        }

        public Task<TResponse> InvokeAsync(TRequest request, bool throwOnError = false)
        {
            return ThreadHelper.RunInNewThreadAsync(async () =>
            {
                using (var context = new ComponentOperationContext(_component, OperationName))
                {
                    // on operation start
                    try
                    {
                        await _component.RunOnComponentOperationStartAsync(context);
                    }
                    catch (Exception ex)
                    {
                        context.Log.Error(LogContextEnum.Component, "Operation start handler error.", ex, callerName: null);
                    }

                    // run operation
                    TResponse response;
                    try
                    {
                        Assure.ArgumentNotNull(request, nameof(request));

                        response = await _implementation(request, context);

                        if (response == null)
                        {
                            throw new InvalidFlowStateException("Operation returned 'null'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        context.Log.Error(LogContextEnum.Component, "Operation error.", ex, callerName: null);

                        response = new TResponse()
                        {
                            Status = ComponentOperationStatusEnum.Error,
                        };
                    }

                    // on operation end
                    try
                    {
                        await _component.RunOnComponentOperationEndAsync(context);
                    }
                    catch (Exception ex)
                    {
                        context.Log.Error(LogContextEnum.Component, "Operation end handler error.", ex, callerName: null);
                    }

                    if (response.Status == ComponentOperationStatusEnum.Error
                        && throwOnError)
                    {
                        throw new ComponentOperationException($"'{_component.FullName}'.{OperationName} operation error.");
                    }

                    return response;
                }
            });
        }
    }
}