using System;
using System.Linq;

namespace KioskBrains.Waf.Actions.Processing
{
    internal static class WafActionNameHelper
    {
        public static readonly string[] SupportedActionNameMethods =
            {
                "Get",
                "Post",
                "Delete",
            };

        public static WafActionName GetActionNameByType(Type actionType)
        {
            var actionFullName = actionType.Name;
            var method = SupportedActionNameMethods
                .FirstOrDefault(x => actionFullName.EndsWith(x));
            if (method == null)
            {
                throw new WafActionContractException($"Invalid action name '{actionFullName}'. Action name should end with: {string.Join(", ", SupportedActionNameMethods)}.");
            }
            if (actionFullName.Length == method.Length)
            {
                throw new WafActionContractException($"Invalid action name '{actionFullName}'. Action name should has format 'XxxMethod'.");
            }
            var actionName = actionFullName.Substring(0, actionFullName.Length - method.Length);
            return new WafActionName()
                {
                    FullName = actionFullName,
                    Name = actionName,
                    Method = method.ToUpper(),
                };
        }
    }
}