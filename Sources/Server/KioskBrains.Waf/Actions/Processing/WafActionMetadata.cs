using KioskBrains.Waf.Actions.Common;

namespace KioskBrains.Waf.Actions.Processing
{
    internal class WafActionMetadata
    {
        public WafActionName WafActionName { get; set; }

        public ActionAuthorizeAttribute ActionAuthorizeAttribute { get; set; }
    }
}