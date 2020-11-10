namespace KioskBrains.Waf
{
    public static class WafConstants
    {
        public const string WafApiUrlRoot = "api";

        public const string WafApiGetRequestParameterName = "request";

        public const string WafRouteTemplatePath = WafApiUrlRoot + "/{action}/{*extra}";
    }
}