using System;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.AspNetCore.Http;

namespace KioskBrains.Server.EK.Integration.Middleware
{
    public class EkApiRoleProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EkApiRoleProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public EkApiRoleEnum GetEkApiRole()
        {
            var httpRequest = _httpContextAccessor.HttpContext?.Request;
            if (httpRequest == null)
            {
                throw new InvalidOperationException("HttpContext.Request is null.");
            }

            var requestApiKey = (string)httpRequest.Headers["X-API-KEY"];
            switch (requestApiKey)
            {
                case "BH9T42QWUA90SPFBFK2G7MB9L8YN31JJ":
                    return EkApiRoleEnum.EkTest;

                case "X6TWJ7RJKRWR9KVIR3FIFJJ38AA2OMRQ":
                    return EkApiRoleEnum.EkLive;

                case "1DADY93S7U0EI06D91X6FR8EUDP49SLP":
                    return EkApiRoleEnum.Admin;

                default:
                    throw new ActionAuthorizationException();
            }
        }
    }
}