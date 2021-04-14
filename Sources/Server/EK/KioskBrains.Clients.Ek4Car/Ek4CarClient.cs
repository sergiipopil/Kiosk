using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.Ek4Car.Models;
using KioskBrains.Common.Contracts;
using KioskBrains.Server.Common.Constants;
using KioskBrains.Server.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Web;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace KioskBrains.Clients.Ek4Car
{
    public class Ek4CarClient
    {
        private readonly Ek4CarClientSettings _settings;

        private readonly IServiceProvider _serviceProvider;

        public Ek4CarClient(
            IOptions<Ek4CarClientSettings> settings,
            IServiceProvider serviceProvider)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));

            _serviceProvider = serviceProvider;
        }

        private async Task<TData> SendPostRequestAsync<TData>(
            string actionPath,
            object request,
            CancellationToken cancellationToken)
            where TData : class, new()
        {
            Assure.ArgumentNotNull(actionPath, nameof(actionPath));
            if (!actionPath.StartsWith("/"))
            {
                actionPath = "/" + actionPath;
            }

            // request new integration log manager
            var integrationLogManager = _serviceProvider.GetRequiredService<IIntegrationLogManager>();
            integrationLogManager.StartNewLogRecord(ExternalSystems.EK, IntegrationRequestDirectionEnum.FromKioskBrainsServer);
            integrationLogManager.LogToRequest("Request", $"POST {actionPath}");

            try
            {
                string responseBody;
                try
                {
                    var requestJson = JsonConvert.SerializeObject(request);

                    integrationLogManager.LogToRequest("Body", requestJson);

                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Add("X-API-KEY", "KWLG@10213FKWll@)=!kkf!_dfWN");
                        var httpResponse = await httpClient.PostAsync(
                            new Uri(_settings.ApiUrl + actionPath),
                            new StringContent(requestJson, Encoding.UTF8, "application/json"),
                            cancellationToken);
                        responseBody = await httpResponse.Content.ReadAsStringAsync();
                        
                        integrationLogManager.LogToResponse("StatusCode", ((int)httpResponse.StatusCode).ToString());
                        integrationLogManager.LogToResponse("Body", responseBody);

                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            throw new Ek4CarRequestException($"Request to API failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Ek4CarRequestException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new Ek4CarRequestException("Request to API failed, no response.", ex);
                }

                ResponseWrapper<TData> response;
                try
                {
                    response = JsonConvert.DeserializeObject<ResponseWrapper<TData>>(responseBody);
                }
                catch (Exception ex)
                {
                    throw new Ek4CarRequestException("Bad format of API response.", ex);
                }

                if (response == null)
                {
                    throw new Ek4CarRequestException("API response is null.");
                }

                if (!response.Success)
                {
                    throw new Ek4CarRequestException($"API request failed, error: '{response.Error?.Message}'.");
                }

                await integrationLogManager.CompleteLogRecordAsync();

                return response.Data;
            }
            catch (OperationCanceledException)
            {
                // cancelled - do not save log record

                throw;
            }
            catch (Exception ex)
            {
                integrationLogManager.LogToResponse("Error", ex);

                await integrationLogManager.CompleteLogRecordAsync();

                throw;
            }
        }

        public Task<VerifyPhoneNumberResponse> VerifyPhoneNumberAsync(
            VerifyPhoneNumberRequest request,
            CancellationToken cancellationToken)
        {
            return SendPostRequestAsync<VerifyPhoneNumberResponse>(
                "/verify-phone-number",
                request,
                cancellationToken);
        }

        public Task SendOrderAsync(
            Order order,
            CancellationToken cancellationToken)
        {
            return SendPostRequestAsync<EmptyData>(
                "/orders",
                order,
                cancellationToken);
        }
    }
}