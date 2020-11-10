using System;
using System.Threading.Tasks;

namespace KioskBrains.Waf.Actions.Common
{
    public abstract class WafAction<TRequest, TResponse> : IWafActionInternal
    {
        public abstract bool IsTransactionEnabled { get; }

        public virtual bool AllowAnonymous { get; } = false;

        public abstract Task<TResponse> ExecuteAsync(TRequest request);

        #region Internals, IWafActionInternal Implementation

        internal static Type RequestTypeInternal { get; } = typeof(TRequest);

        internal static Type ResponseTypeInternal { get; } = typeof(TResponse);

        async Task<object> IWafActionInternal.ExecuteAsync(object request)
        {
            return await ExecuteAsync((TRequest)request);
        }

        #endregion
    }
}