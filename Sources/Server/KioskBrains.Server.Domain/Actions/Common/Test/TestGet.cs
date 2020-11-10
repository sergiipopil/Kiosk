using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Waf.Actions.Common;

namespace KioskBrains.Server.Domain.Actions.Common.Test
{
    public class TestGet : WafActionGet<EmptyRequest, TestGetResponse>
    {
        public override bool AllowAnonymous => true;

        public override async Task<TestGetResponse> ExecuteAsync(EmptyRequest request)
        {
            var resultBuilder = new StringBuilder("IP: ");
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var responseString = await httpClient.GetStringAsync(new Uri("https://api.myip.com"));
                    resultBuilder.Append(responseString);
                }
            }
            catch (Exception ex)
            {
                resultBuilder.Append($"Exception {ex.Message}");
            }

            return new TestGetResponse()
                {
                    Result = resultBuilder.ToString(),
                };
        }
    }
}