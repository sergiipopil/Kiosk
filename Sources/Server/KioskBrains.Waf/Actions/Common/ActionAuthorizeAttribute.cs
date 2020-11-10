using System;
using System.Linq;
using KioskBrains.Waf.Security;

namespace KioskBrains.Waf.Actions.Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActionAuthorizeAttribute : Attribute
    {
        private string _roles;

        public string Roles
        {
            get => _roles;
            set
            {
                _roles = value;
                if (!string.IsNullOrEmpty(_roles))
                {
                    _roleArray = _roles
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToArray();
                }
                else
                {
                    _roleArray = null;
                }
            }
        }

        private string[] _roleArray;

        public virtual bool Authorize(ICurrentUser currentUser)
        {
            if (currentUser == null)
            {
                return false;
            }

            if (_roleArray != null)
            {
                if (_roleArray.All(x => !currentUser.IsInRole(x)))
                {
                    return false;
                }
            }

            return true;
        }
    }
}