using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Waf.Actions.Common;

namespace KioskBrains.Server.Domain.Actions.Common.Ping
{
    public class PingGet : WafActionGet<EmptyRequest, EmptyResponse>
    {
        public override Task<EmptyResponse> ExecuteAsync(EmptyRequest request)
        {
            return Task.FromResult(new EmptyResponse());
        }
    }
}