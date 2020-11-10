using System;
using System.Security.Claims;

namespace KioskBrains.Waf.Security
{
    internal class WebCurrentUser : ICurrentUser
    {
        private readonly ClaimsPrincipal _claimsPrincipal;

        public WebCurrentUser(ClaimsPrincipal claimsPrincipal)
        {
            _claimsPrincipal = claimsPrincipal;
            if (!_claimsPrincipal.Identity.IsAuthenticated)
            {
                throw new InvalidOperationException("Passed identity is not authenticated.");
            }

            var idString = _claimsPrincipal.Identity.Name;
            if (!int.TryParse(idString, out var id))
            {
                throw new WafUserContractException($"Tokenized username '{idString}' is not valid id (should be integer).");
            }
            Id = id;
        }

        public int Id { get; }

        public bool IsInRole(string role)
        {
            return _claimsPrincipal.IsInRole(role);
        }

        public TValueType GetClaimValue<TValueType>(string claimType, bool mandatory = true)
        {
            var claim = _claimsPrincipal.FindFirst(claimType);
            if (claim == null)
            {
                if (mandatory)
                {
                    throw new WafUserContractException($"User token doesn't contain claim of type '{claimType}'.");
                }
                return default(TValueType);
            }
            var value = claim.Value;
            try
            {
                return (TValueType)Convert.ChangeType(value, typeof(TValueType));
            }
            catch (Exception ex)
            {
                throw new WafUserContractException($"Tokenized {claimType} is not of type {typeof(TValueType).Name} (error: {ex.Message}).");
            }
        }
    }
}