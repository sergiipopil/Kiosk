using System;
using System.Collections.Generic;
using System.Text;
using KioskBrains.Server.Domain.Entities;
using System.Threading.Tasks;

namespace KioskBrains.Server.Domain.ServiceInterfaces
{
    public interface ITokenService
    {
        Task<TokenInfo> GetToken(TokenType tokenType);
        Task SetToken(string token, TokenType tokenType, Guid by);
    }
}
