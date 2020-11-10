using System.Linq;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Waf.Actions.Common;

namespace KioskBrains.Server.Domain.Security
{
    public class AuthorizeUserAttribute : ActionAuthorizeAttribute
    {
        public AuthorizeUserAttribute(params UserRoleEnum[] allowedRoles)
        {
            if (allowedRoles.Length > 0)
            {
                Roles = string.Join(',', allowedRoles.Select(x => x.ToString()));
            }
        }
    }
}