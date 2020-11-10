using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Common.Contracts;
using KioskBrains.Kiosk.Helpers.Text;
using KioskBrains.Kiosk.Helpers.Threads;
using Newtonsoft.Json;

namespace KioskBrains.Kiosk.Core.ServerApi
{
    public class ServerApiProxy
    {
        #region Singleton

        public static ServerApiProxy Current { get; } = new ServerApiProxy();

        private ServerApiProxy()
        {
        }

        #endregion

        private Uri _serverUri;

        private string _serialKey;

        private const string ApiRootPath = "/api";

        internal void Initialize(Uri serverUri, string serialKey)
        {
            Assure.ArgumentNotNull(serverUri, nameof(serverUri));
            Assure.ArgumentNotNull(serialKey, nameof(serialKey));

            _serverUri = serverUri;
            _serialKey = serialKey;
        }

        #region API Invocation

        private Task<TResponse> GetInternalAsync<TResponse>(
            string path,
            object request,
            CancellationToken cancellationToken,
            [CallerMemberName] string callerName = "")
        {
            Assure.ArgumentNotNull(request, nameof(request));

            return ThreadHelper.RunInBackgroundThreadAsync(async () =>
            {
                var requestJson = JsonConvert.SerializeObject(request);
                try
                {
                    using (var httpClient = ConstructHttpClient())
                    {
                        var httpResponse = await httpClient.GetAsync(ApiRootPath + path + "?request=" + requestJson, cancellationToken);

                        return await ProcessApiResponseAsync<TResponse>(httpResponse);
                    }
                }
                catch (Exception ex)
                {
                    // check if server error caused by cancellation token
                    cancellationToken.ThrowIfCancellationRequested();

                    var errorMessage = $"Server API request {callerName}({requestJson.FirstNSymbols(100)}) failed: {ex.Message}";
                    throw new ServerApiException(errorMessage, ex);
                }
            });
        }

        private Task<TResponse> PostInternalAsync<TResponse>(
            string path,
            object request,
            CancellationToken cancellationToken,
            bool suppressRequestLogging = false,
            [CallerMemberName] string callerName = "")
        {
            Assure.ArgumentNotNull(request, nameof(request));

            return ThreadHelper.RunInBackgroundThreadAsync(async () =>
            {
                var requestJson = JsonConvert.SerializeObject(request);
                try
                {
                    using (var httpClient = ConstructHttpClient())
                    {
                        var httpResponse = await httpClient.PostAsync(ApiRootPath + path, new StringContent(requestJson, Encoding.UTF8, "application/json"), cancellationToken);

                        return await ProcessApiResponseAsync<TResponse>(httpResponse);
                    }
                }
                catch (Exception ex)
                {
                    // check if server error caused by cancellation token
                    cancellationToken.ThrowIfCancellationRequested();

                    var requestLogString = suppressRequestLogging ? "[hidden]" : requestJson.FirstNSymbols(100);
                    var errorMessage = $"Server API request {callerName}({requestLogString}) failed: {ex.Message}";
                    throw new ServerApiException(errorMessage, ex);
                }
            });
        }

        private async Task<TResponse> ProcessApiResponseAsync<TResponse>(HttpResponseMessage httpResponse)
        {
            const string CommunicationError = "Server API communication error";

            var responseBody = await httpResponse.Content.ReadAsStringAsync();
            var mediaType = httpResponse.Content.Headers.ContentType?.MediaType;
            var isJsonMediaType = mediaType == JsonMediaType;

            if (!httpResponse.IsSuccessStatusCode)
            {
                if (string.IsNullOrEmpty(responseBody) || !isJsonMediaType)
                {
                    throw new ServerApiException($"{CommunicationError}: {httpResponse.StatusCode} ({(int)httpResponse.StatusCode}).");
                }

                var errorResponse = DeserializeApiResponse<TResponse>(responseBody);
                throw new ServerApiException($"Server API error: {errorResponse.Meta?.ErrorMessage} ({errorResponse.Meta?.Code}).");
            }

            if (string.IsNullOrEmpty(responseBody))
            {
                throw new ServerApiException($"{CommunicationError}: empty response.");
            }

            if (!isJsonMediaType)
            {
                throw new ServerApiException($"{CommunicationError}: response media is not '{JsonMediaType}' (actual: '{mediaType}').");
            }

            var response = DeserializeApiResponse<TResponse>(responseBody);
            if (response.Data == null)
            {
                throw new ServerApiException($"{CommunicationError}: '{nameof(response)}.{nameof(response.Data)}' is null.");
            }

            return response.Data;
        }

        private ApiResponseWrapper<TResponse> DeserializeApiResponse<TResponse>(string responseBody)
        {
            try
            {
                return JsonConvert.DeserializeObject<ApiResponseWrapper<TResponse>>(responseBody);
            }
            catch (Exception ex)
            {
                throw new ServerApiException($"Server API response deserialization error: {ex.Message} (response: {responseBody.FirstNSymbols(100)}).");
            }
        }

        private const string JsonMediaType = "application/json";

        private HttpClient ConstructHttpClient()
        {
            if (_serverUri == null)
            {
                throw new ServerApiException($"{nameof(ServerApiProxy)} is not initialized. Run {nameof(Initialize)} first.");
            }

            var httpClient = new HttpClient
            {
                BaseAddress = _serverUri,
            };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
            if (_token != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }

            httpClient.DefaultRequestHeaders.Add("X-Api-Request", "true");
            // todo: implement encoding support for gzip
            //httpClient.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate");
            return httpClient;
        }

        #endregion

        #region Auth Session

        private string _token;

        private async Task EnsureAuthSessionAsync()
        {
            if (_token != null)
            {
                return;
            }

            var response = await PostInternalAsync<KioskLoginPostResponse>(
                "/kiosk-login",
                new KioskLoginPostRequest()
                {
                    SerialKey = _serialKey,
                },
                CancellationToken.None,
                suppressRequestLogging: true);
            _token = response.Token;
        }

        #endregion

        public async Task<KioskConfigurationGetResponse> GetKioskConfigurationAsync(EmptyRequest request)
        {
            await EnsureAuthSessionAsync();

            return await GetInternalAsync<KioskConfigurationGetResponse>("/kiosk-configuration", request, CancellationToken.None);
        }

        public async Task<KioskSyncPostResponse> SyncAsync(KioskSyncPostRequest request)
        {
            await EnsureAuthSessionAsync();

            return await PostInternalAsync<KioskSyncPostResponse>("/kiosk-sync", request, CancellationToken.None);
        }

        public async Task<TResponse> GetAsync<TResponse>(string actionName, object request, CancellationToken cancellationToken)
        {
            await EnsureAuthSessionAsync();

            return await GetInternalAsync<TResponse>(actionName, request, cancellationToken);
        }

        public async Task<TResponse> PostAsync<TResponse>(string actionName, object request, CancellationToken cancellationToken)
        {
            await EnsureAuthSessionAsync();

            return await PostInternalAsync<TResponse>(actionName, request, cancellationToken);
        }
    }
}