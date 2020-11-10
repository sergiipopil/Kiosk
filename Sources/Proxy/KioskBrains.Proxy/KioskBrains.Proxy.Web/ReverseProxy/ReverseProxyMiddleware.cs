using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KioskBrains.Proxy.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace KioskBrains.Proxy.Web.ReverseProxy
{
    public class ReverseProxyMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly TimeSpan _requestTimeout;

        public ReverseProxyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _requestTimeout = TimeSpan.Parse(configuration["ProxyTimeout"]);
        }

        public const string PassAction = "/pass";

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider)
        {
            try
            {
                var requestPath = context.Request.Path.Value;
                if (!requestPath.StartsWith(PassAction, StringComparison.OrdinalIgnoreCase))
                {
                    await SetErrorResponseAsync(context, 400, "Bad Request");
                    return;
                }

                // read request
                PassRequest request;
                try
                {
                    request = await ReadPostRequestAsync<PassRequest>(context.Request);
                }
                catch (Exception ex)
                {
                    await SetErrorResponseAsync(context, 400, $"Bad Request ({ex.Message})");
                    return;
                }

                PassResponse response;
                try
                {
                    var receivedResponse = await SendRequestAsync(request);
                    response = new PassResponse()
                        {
                            IsReceived = true,
                            ReceivedResponse = receivedResponse,
                        };
                }
                catch (Exception ex)
                {
                    response = new PassResponse()
                        {
                            IsReceived = false,
                            ErrorMessage = $"Exception: {ex}",
                        };
                }

                // success
                context.Response.StatusCode = 200;
                var responseBody = JsonConvert.SerializeObject(response);
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(responseBody);
            }
            catch (Exception ex)
            {
                await SetErrorResponseAsync(context, 500, ex.Message);
                return;
            }
        }

        private async Task SetErrorResponseAsync(HttpContext context, int responseCode, string message)
        {
            context.Response.StatusCode = responseCode;
            context.Response.ContentType = "plain/text";
            await context.Response.WriteAsync($"{responseCode} {message}");
        }

        private async Task<TRequest> ReadPostRequestAsync<TRequest>(HttpRequest httpRequest)
        {
            // get serialized request
            string serializedRequest;
            using (var streamReader = new StreamReader(httpRequest.Body))
            {
                serializedRequest = await streamReader.ReadToEndAsync();
            }

            if (serializedRequest == null)
            {
                throw new ArgumentException("Request is empty.");
            }

            // deserialize request
            TRequest request;
            try
            {
                request = JsonConvert.DeserializeObject<TRequest>(serializedRequest);
                if (request == null)
                {
                    throw new ArgumentException("Request is empty.");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid request structure: '{ex.Message}'.");
            }

            return request;
        }

        private async Task<HttpResponseData> SendRequestAsync(PassRequest request)
        {
            var requestUri = new Uri(request.Url);
            using (var httpClient = new HttpClient())
            {
                httpClient.Timeout = _requestTimeout;

                var httpRequestMessage = new HttpRequestMessage()
                    {
                        RequestUri = requestUri,
                        Method = new HttpMethod(request.Method),
                    };

                if (request.RequestHeaders != null)
                {
                    httpRequestMessage.Headers.Clear();

                    foreach (var (name, value) in request.RequestHeaders)
                    {
                        httpRequestMessage.Headers.Add(name, value);
                    }
                }

                if (request.ContentBody != null)
                {
                    httpRequestMessage.Content = new StringContent(request.ContentBody);

                    if (request.ContentHeaders != null)
                    {
                        httpRequestMessage.Content.Headers.Clear();

                        foreach (var (name, value) in request.ContentHeaders)
                        {
                            httpRequestMessage.Content.Headers.Add(name, value);
                        }
                    }
                }

                var response = await httpClient.SendAsync(httpRequestMessage);

                var responseData = new HttpResponseData()
                    {
                        StatusCode = (int)response.StatusCode,
                        ResponseHeaders = new Dictionary<string, string>(),
                        ContentHeaders = new Dictionary<string, string>(),
                    };

                foreach (var (name, values) in response.Headers)
                {
                    responseData.ResponseHeaders.Add(name, values.FirstOrDefault());
                }

                if (response.Content != null)
                {
                    responseData.ContentBody = await response.Content.ReadAsStringAsync();

                    foreach (var (name, values) in response.Content.Headers)
                    {
                        responseData.ContentHeaders.Add(name, values.FirstOrDefault());
                    }
                }

                return responseData;
            }
        }
    }
}