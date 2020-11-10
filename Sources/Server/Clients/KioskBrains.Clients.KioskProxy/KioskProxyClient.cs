using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Proxy.Common.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace KioskBrains.Clients.KioskProxy
{
    public class KioskProxyClient
    {
        private readonly KioskProxyClientSettings _settings;

        public KioskProxyClient(IOptions<KioskProxyClientSettings> settings)
        {
            _settings = settings.Value;

            Assure.ArgumentNotNull(_settings, nameof(_settings));
            Assure.ArgumentNotNull(_settings.Url, nameof(_settings.Url));
            Assure.ArgumentNotNull(_settings.ProxyKey, nameof(_settings.ProxyKey));
        }

        public async Task<HttpResponseData> PassAsync(PassRequest passRequest)
        {
            Assure.ArgumentNotNull(passRequest, nameof(passRequest));

            string responseBody;
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Add("ProxyKey", _settings.ProxyKey);

                    var requestJson = JsonConvert.SerializeObject(passRequest);
                    var httpResponse = await httpClient.PostAsync(
                        new Uri(_settings.Url + "/pass"),
                        new StringContent(requestJson, Encoding.UTF8, "application/json"));

                    responseBody = await httpResponse.Content.ReadAsStringAsync();
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new KioskProxyRequestException($"Request to proxy failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                    }
                }
            }
            catch (KioskProxyRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new KioskProxyRequestException("Request to proxy failed, no response.", ex);
            }

            PassResponse response;
            try
            {
                response = JsonConvert.DeserializeObject<PassResponse>(responseBody);
            }
            catch (Exception ex)
            {
                throw new KioskProxyRequestException("Bad format of proxy response.", ex);
            }

            if (response == null)
            {
                throw new KioskProxyRequestException("Empty proxy response.");
            }

            if (!response.IsReceived)
            {
                throw new KioskProxyRequestException($"Request to target resource failed, no response, error: {response.ErrorMessage}");
            }

            if (response.ReceivedResponse == null)
            {
                throw new KioskProxyRequestException($"Empty proxy response {nameof(response.ReceivedResponse)}.");
            }

            return response.ReceivedResponse;
        }
    }
}