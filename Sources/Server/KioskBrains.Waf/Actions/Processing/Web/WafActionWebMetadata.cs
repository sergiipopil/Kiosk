using System;

namespace KioskBrains.Waf.Actions.Processing.Web
{
    internal class WafActionWebMetadata
    {
        public Type ActionType { get; set; }

        public Type RequestType { get; set; }
    }
}