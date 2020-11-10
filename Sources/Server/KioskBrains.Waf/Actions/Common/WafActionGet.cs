namespace KioskBrains.Waf.Actions.Common
{
    public abstract class WafActionGet<TRequest, TResponse> : WafAction<TRequest, TResponse>
    {
        public override bool IsTransactionEnabled { get; } = false;
    }
}