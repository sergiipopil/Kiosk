using System;
using System.Security.Claims;
using KioskBrains.Server.Domain.Entities;
using KioskBrains.Waf.Security;

namespace KioskBrains.Server.Domain.Security
{
    public class CurrentUser
    {
        public int Id { get; set; }

        public UserRoleEnum Role { get; set; }

        public bool IsAppClient => Role == UserRoleEnum.KioskApp
                                   || Role == UserRoleEnum.IntegrationClient;

        public bool IsGlobalPortalUser => Role == UserRoleEnum.GlobalAdmin
                                          || Role == UserRoleEnum.GlobalSupport;

        public int CustomerId { get; set; }

        public string TimeZoneName { get; internal set; }

        internal static CurrentUser GetByAuthenticatedUser(ICurrentUser currentUser)
        {
            return new CurrentUser()
                {
                    Id = currentUser.Id,
                    Role = Enum.Parse<UserRoleEnum>(currentUser.GetClaimValue<string>(ClaimsIdentity.DefaultRoleClaimType)),
                    CustomerId = currentUser.GetClaimValue<int>(AppClaimsIdentity.CustomerId),
                    TimeZoneName = currentUser.GetClaimValue<string>(AppClaimsIdentity.TimeZoneName, mandatory: false),
                };
        }

        internal static CurrentUser BuildKioskAppUser(int kioskId, int customerId, string timeZoneName)
        {
            return new CurrentUser()
                {
                    Id = kioskId,
                    Role = UserRoleEnum.KioskApp,
                    CustomerId = customerId,
                    TimeZoneName = timeZoneName
                };
        }

        internal static CurrentUser BuildIntegrationClientUser(int customerId, string timeZoneName)
        {
            return new CurrentUser()
                {
                    Id = customerId,
                    Role = UserRoleEnum.IntegrationClient,
                    CustomerId = customerId,
                    TimeZoneName = timeZoneName
                };
        }

        internal static CurrentUser BuildPortalUser(int kioskId, UserRoleEnum portalUserRole, int customerId, bool isSystemCustomer, string timeZoneName)
        {
            UserRoleEnum userRole;
            switch (portalUserRole)
            {
                case UserRoleEnum.CustomerAdmin:
                    userRole = isSystemCustomer ? UserRoleEnum.GlobalAdmin : UserRoleEnum.CustomerAdmin;
                    break;
                case UserRoleEnum.CustomerSupport:
                    userRole = isSystemCustomer ? UserRoleEnum.GlobalSupport : UserRoleEnum.CustomerSupport;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portalUserRole), portalUserRole, null);
            }

            return new CurrentUser()
                {
                    Id = kioskId,
                    Role = userRole,
                    CustomerId = customerId,
                    TimeZoneName = timeZoneName
                };
        }
    }
}