namespace KioskBrains.Waf.Actions.Processing.Web
{
    public class WafActionResponseMeta
    {
        public WafActionResponseMeta(WafActionResponseCodeEnum code, string errorMessage = null)
        {
            Code = code;
            ErrorMessage = errorMessage;
        }

        public WafActionResponseCodeEnum Code { get; set; }

        public string ErrorMessage { get; set; }
    }
}