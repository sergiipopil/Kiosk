using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using KioskBrains.Server.Domain.Entities.DbStorage;
using KioskBrains.Server.Domain.Security;
using KioskBrains.Waf.Actions.Common;
using Microsoft.EntityFrameworkCore;

namespace KioskBrains.Server.Domain.Actions.Portal.PortalLogin
{
    public class PortalLoginPost : WafActionPost<PortalLoginPostRequest, PortalLoginPostResponse>
    {
        public override bool AllowAnonymous => true;

        private readonly KioskBrainsContext _dbContext;

        public PortalLoginPost(KioskBrainsContext dbContext)
        {
            _dbContext = dbContext;
        }

        public override async Task<PortalLoginPostResponse> ExecuteAsync(PortalLoginPostRequest request)
        {
            var passwordHash = PasswordHelper.GetPasswordHash(request.Password);
            var userInfo = await _dbContext.PortalUsers
                .Where(x => x.Username == request.Username
                            && x.PasswordHash == passwordHash)
                .Select(x => new
                    {
                        x.Id,
                        x.CustomerId,
                        IsSystemCustomer = x.Customer.IsSystem,
                        x.Role,
                        x.FullName,
                        x.Customer.TimeZoneName
                    })
                .FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new AuthenticationException("Invalid username or password.");
            }

            var currentUser = CurrentUser.BuildPortalUser(
                userInfo.Id,
                userInfo.Role,
                userInfo.CustomerId,
                userInfo.IsSystemCustomer,
                userInfo.TimeZoneName);

            var token = new TokenGenerator().Generate(currentUser);

            return new PortalLoginPostResponse()
                {
                    Token = token,
                    User = new PortalUserInfo()
                        {
                            FullName = userInfo.FullName,
                        }
                };
        }
    }
}