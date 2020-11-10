using System.Threading.Tasks;

namespace KioskBrains.Waf.Actions.Common
{
    /// <summary>
    /// Interface to simplify an internal search and processing of actions.
    /// </summary>
    internal interface IWafActionInternal
    {
        bool IsTransactionEnabled { get; }

        bool AllowAnonymous { get; }

        Task<object> ExecuteAsync(object request);
    }
}