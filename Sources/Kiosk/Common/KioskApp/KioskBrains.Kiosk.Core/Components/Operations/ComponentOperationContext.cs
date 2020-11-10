using System;
using KioskBrains.Common.Contracts;

namespace KioskBrains.Kiosk.Core.Components.Operations
{
    public class ComponentOperationContext : IDisposable
    {
        public string OperationName { get; }

        public ComponentOperationLogger Log { get; }

        internal ComponentOperationContext(ComponentBase component, string operationName)
        {
            Assure.ArgumentNotNull(component, nameof(component));
            Assure.ArgumentNotNull(operationName, nameof(operationName));

            OperationName = operationName;
            Log = new ComponentOperationLogger(component.FullName, OperationName);
        }

        public void Dispose()
        {
            Log?.Dispose();
        }
    }
}