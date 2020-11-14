using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using KioskBrains.Clients.YandexTranslate.Models;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Newtonsoft.Json.Linq;
using System.Text;
using KioskBrains.Common.Contracts;
using System.Runtime.Serialization;

namespace KioskBrains.Clients.YandexTranslate
{
    [Serializable]
    internal class AllegroPlRequestException : Exception
    {
        public AllegroPlRequestException()
        {
        }

        public AllegroPlRequestException(string message) : base(message)
        {
        }

        public AllegroPlRequestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected AllegroPlRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
    public class YandexTranslateClient
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly YandexTranslateClientSettings _settings;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable
        private string _accessToken;
        private static readonly TimeSpan AccessTokenDuration = TimeSpan.FromHours(6);

        private readonly object _authRequestLocker = new object();

        private bool _isAuthRequestInProgress;
       
        private DateTime? _accessTokenTime;

        public YandexTranslateClient(IOptions<YandexTranslateClientSettings> settings)
        {
            _settings = settings.Value;
            Assure.ArgumentNotNull(_settings, nameof(_settings));
           
        }

        private bool IsAuthSessionExpired(DateTime now)
        {
            var accessTokenTime = _accessTokenTime;
            return accessTokenTime == null
                   || accessTokenTime.Value + AccessTokenDuration < now;
        }

        private async Task EnsureAuthSessionAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            
            if (!IsAuthSessionExpired(now))
            {
                return;
            }

            lock (_authRequestLocker)
            {
                if (_isAuthRequestInProgress)
                {
                    if (_accessToken == null)
                    {
                        // possible for parallel requests after app started
                        throw new AllegroPlRequestException("Auth session is being initialized...");
                    }

                    // AccessTokenDuration is much lower than actual one, so old access token can be used even while new one is requested
                    return;
                }

                _isAuthRequestInProgress = true;
            }

            try
            {
                string responseBody;
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Clear();
                        //httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.ApiKey}");
                        var httpResponse = await httpClient.PostAsync(
                            "https://iam.api.cloud.yandex.net/iam/v1/tokens",
                            new StringContent($"{{ \"yandexPassportOauthToken\":\"{ _settings.ApiKey }\" }}", Encoding.UTF8, "application/json"),
                                                    cancellationToken);
                        responseBody = await httpResponse.Content.ReadAsStringAsync();
                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            throw new AllegroPlRequestException($"Request to auth yandex API failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (AllegroPlRequestException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new AllegroPlRequestException("Request to auth API failed, no response.", ex);
                }

                string accessToken;
                try
                {
                    const string AccessTokenProperty = "iamToken";
                    var response = JsonConvert.DeserializeObject<JObject>(responseBody);
                    accessToken = response[AccessTokenProperty].Value<string>();
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        throw new Exception($"'{AccessTokenProperty}' is null or empty.");
                    }
                }
                catch (Exception ex)
                {
                    throw new AllegroPlRequestException("Bad format of auth API response.", ex);
                }
                
                _accessToken = accessToken;
                _accessTokenTime = now;
            }
            finally
            {
                lock (_authRequestLocker)
                {
                    _isAuthRequestInProgress = false;
                }
            }
        }
        

        private const int MaxTextLength = 10_000;

        public async Task<string[]> TranslateAsync(
            string[] texts,
            string fromLanguageCode,
            string toLanguageCode,
            CancellationToken cancellationToken)
        {
            await EnsureAuthSessionAsync(cancellationToken);
            if (texts == null
                || texts.All(x => string.IsNullOrWhiteSpace(x)))
            {
                return texts;
            }

            Assure.ArgumentNotNull(fromLanguageCode, nameof(fromLanguageCode));
            Assure.ArgumentNotNull(toLanguageCode, nameof(toLanguageCode));



            string responseBody;
            try
            {
                var apiKey = _settings.ApiKey;
                var requestUrl = $"https://translate.api.cloud.yandex.net/translate/v2/translate";
                /*var httpContent = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    ["sourceLanguageCode"] = Languages.PolishCode,
                    ["targetLanguageCode"] = Languages.RussianCode,                   
                    ["folderId"] = "b1gr992001hh7bimbr42",
                    ["format"] = "PLAIN_TEXT",
                    ["texts"] = JsonConvert.SerializeObject(texts)    
                });*/

                var str = $"{{ \"sourceLanguageCode\":\"{ fromLanguageCode.ToLower() }\", \"targetLanguageCode\": \"{toLanguageCode.ToLower()}\", \"format\": \"PLAIN_TEXT\", \"folderId\": \"b1gr992001hh7bimbr42\", \"texts\": {JsonConvert.SerializeObject(texts)} }}";
                var httpContent = new StringContent(str, Encoding.UTF8, "application/json");

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    //httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_accessToken}");
                    var httpResponse = await httpClient.PostAsync(
                        requestUrl,
                        httpContent,
                        cancellationToken);
                    responseBody = await httpResponse.Content.ReadAsStringAsync();
                    if (!httpResponse.IsSuccessStatusCode)
                    {
                        throw new YandexTranslateRequestException($"Request to API failed, response code {(int)httpResponse.StatusCode}, body: {responseBody}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (YandexTranslateRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return texts;
                throw new YandexTranslateRequestException("Request to API failed, no response.", ex);
            }

            JObject response;
            string[] res;
            try
            {
                response = JsonConvert.DeserializeObject<JObject>(responseBody);
                // todo: fix this in all clients - move out of try-catch
                if (response == null)
                {
                    throw new YandexTranslateRequestException("API response is null.");
                }

                res = response["translations"].ToList().Select(x => x["text"].ToString()).ToArray();
            }
            catch (Exception ex)
            {
                throw new YandexTranslateRequestException("Bad format of API response.", ex);
            }

            if (res.Any())
            {
                return res;
            }

            throw new YandexTranslateRequestException("API response doesn't contain any text.");

            //return translatedTexts;
        }
    }
}