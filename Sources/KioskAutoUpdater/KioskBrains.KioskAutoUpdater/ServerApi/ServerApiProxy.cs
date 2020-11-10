using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Api;
using KioskBrains.Common.Contracts;
using KioskBrains.KioskAutoUpdater.Helpers;
using Newtonsoft.Json;

namespace KioskBrains.KioskAutoUpdater.ServerApi
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

        private async Task<TResponse> GetAsync<TResponse>(string path, object request, [CallerMemberName] string callerName = "")
        {
            Assure.ArgumentNotNull(request, nameof(request));

            var requestJson = JsonConvert.SerializeObject(request);
            try
            {
                using (var httpClient = ConstructHttpClient())
                {
                    var httpResponse = await httpClient.GetAsync(ApiRootPath + path + "?request=" + requestJson);

                    return await ProcessApiResponseAsync<TResponse>(httpResponse);
                }
            }
            catch (Exception ex)
            {
                throw new ServerApiException($"Server API request {callerName}({requestJson.FirstNSymbols(100)}) failed: {ex.Message}", ex);
            }
        }

        private async Task<TResponse> PostAsync<TResponse>(string path, object request, bool suppressRequestLogging = false, [CallerMemberName] string callerName = "")
        {
            Assure.ArgumentNotNull(request, nameof(request));

            var requestJson = JsonConvert.SerializeObject(request);
            try
            {
                using (var httpClient = ConstructHttpClient())
                {
                    var httpResponse = await httpClient.PostAsync(ApiRootPath + path, new StringContent(requestJson, Encoding.UTF8, "application/json"));

                    return await ProcessApiResponseAsync<TResponse>(httpResponse);
                }
            }
            catch (Exception ex)
            {
                var requestLogString = suppressRequestLogging ? "[hidden]" : requestJson.FirstNSymbols(100);
                throw new ServerApiException($"Server API request {callerName}({requestLogString}) failed: {ex.Message}", ex);
            }
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
                    BaseAddress = _serverUri
                };
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
            if (_token != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            }
            httpClient.DefaultRequestHeaders.Add("X-Api-Request", "true");
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
            var response = await PostAsync<KioskLoginPostResponse>(
                "/kiosk-login",
                new KioskLoginPostRequest()
                    {
                        SerialKey = _serialKey,
                    },
                suppressRequestLogging: true);
            _token = response.Token;
        }

        #endregion

        public async Task<KioskVersionGetResponse> GetKioskVersionAsync(EmptyRequest request)
        {
            await EnsureAuthSessionAsync();

            return await GetAsync<KioskVersionGetResponse>("/kiosk-version", request);
        }
    }
}