using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using KioskBrains.Clients.Ek4Car.Models;
using KioskBrains.Common.Contracts;
using KioskBrains.Server.Common.Constants;
using KioskBrains.Server.Common.Services;
using KioskBrains.Server.EK.Integration.Managers;
using KioskBrains.Waf.Helpers.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace KioskBrains.Server.EK.Integration.Middleware
{
    public class EkApiMiddleware
    {
        private readonly RequestDelegate _next;

        public EkApiMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public const string EkApiPath = "/ek/";

        private int _responseStatusCode;

        private string _responseJson;

        public async Task InvokeAsync(HttpContext context, IServiceProvider serviceProvider, IIntegrationLogManager integrationLogManager)
        {
            var requestPath = context.Request.Path.Value;
            if (!requestPath.StartsWith(EkApiPath, StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            try
            {
                integrationLogManager.StartNewLogRecord(ExternalSystems.EK, IntegrationRequestDirectionEnum.ToKioskBrainsServer);

                var method = context.Request.Method;
                integrationLogManager.LogToRequest("Request", $"{method} {requestPath}");

                var apiRoleProvider = serviceProvider.GetRequiredService<EkApiRoleProvider>();
                var apiRole = apiRoleProvider.GetEkApiRole();

                var requestParts = requestPath.Substring(EkApiPath.Length)
                    .Split('/');
                var actionName = requestParts[0].ToLower();
                var idString = requestParts.Length > 1
                    ? requestParts[1]
                    : null;

                if (!AllActionNames.Contains(actionName))
                {
                    SetErrorResponse(400, $"Unknown action '{actionName}'.");
                    return;
                }

                AuthorizeActionAccess(apiRole, actionName);

                var integrationManager = serviceProvider.GetRequiredService<IEkIntegrationManager>();

                // run action
                var isMethodSupported = true;
                object response = null;
                switch (actionName)
                {
                    case UpdatesActionName:
                        if (method == "POST")
                        {
                            var request = await ReadPostRequestAsync<Update[]>(context.Request, integrationLogManager);
                            response = await integrationManager.ApplyUpdatesAsync(request, integrationLogManager);
                        }
                        else
                        {
                            isMethodSupported = false;
                        }

                        break;

                    case KiosksActionName:
                        if (method == "GET")
                        {
                            var id = ParseIdString(idString);
                            response = await integrationManager.GetKioskAsync(id);
                        }
                        else
                        {
                            isMethodSupported = false;
                        }

                        break;

                    case OrdersActionName:
                        if (method == "GET")
                        {
                            var id = ParseIdString(idString);
                            response = await integrationManager.GetOrderAsync(id);
                        }
                        else
                        {
                            isMethodSupported = false;
                        }

                        break;

                    case IndexesActionName:
                    {
                        //var ekSearchManager = serviceProvider.GetRequiredService<EkSearchManager>();
                        switch (method)
                        {
                            case "POST":
                                var request = await ReadPostRequestAsync<SearchIndex>(context.Request, integrationLogManager);
                                //await ekSearchManager.CreateIndexAsync(request.Name, request.Model);
                                break;
                            case "DELETE":
                                //await ekSearchManager.DeleteIndexAsync(idString);
                                break;
                            default:
                                isMethodSupported = false;
                                break;
                        }

                        break;
                    }

                    default:
                        throw new InvalidFlowStateException($"Action '{actionName}'");
                }

                if (!isMethodSupported)
                {
                    SetErrorResponse(400, $"Method {method} is not supported for action '{actionName}'.");
                    return;
                }

                // success
                var apiResponse = new ResponseWrapper<object>()
                    {
                        Success = true,
                        Data = response,
                    };
                _responseJson = JsonConvert.SerializeObject(apiResponse);
                _responseStatusCode = 200;
            }
            catch (Exception ex)
            {
                if (ex is ArgumentException)
                {
                    SetErrorResponse(400, ex.Message);
                    return;
                }

                if (ex is ActionAuthorizationException)
                {
                    SetErrorResponse(401, "Unauthorized");
                    return;
                }

                if (ex is EntityNotFoundException)
                {
                    SetErrorResponse(404, ex.Message);
                    return;
                }

                if (ex is NotImplementedException)
                {
                    SetErrorResponse(501, ex.Message);
                    return;
                }

                SetErrorResponse(500, ex.Message);
                return;
            }
            finally
            {
                // send response
                context.Response.StatusCode = _responseStatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(_responseJson);

                // log response and complete log
                integrationLogManager.LogToResponse("StatusCode", _responseStatusCode.ToString());
                integrationLogManager.LogToResponse("Response", _responseJson);
                await integrationLogManager.CompleteLogRecordAsync();
            }
        }

        private void SetErrorResponse(int responseStatusCode, string message)
        {
            var response = new ResponseWrapper<object>()
                {
                    Success = false,
                    Error = new ResponseErrorInfo()
                        {
                            Message = message,
                        }
                };
            _responseStatusCode = responseStatusCode;
            _responseJson = JsonConvert.SerializeObject(response);
        }

        private const string UpdatesActionName = "updates";

        private const string KiosksActionName = "kiosks";

        private const string OrdersActionName = "orders";

        private const string IndexesActionName = "indexes";

        private static readonly string[] EkActionNames = new[]
            {
                UpdatesActionName,
                KiosksActionName,
                OrdersActionName,
            };

        private static readonly string[] AdminActionNames = new[]
            {
                IndexesActionName,
            };

        private static readonly string[] AllActionNames = EkActionNames
            .Union(AdminActionNames)
            .ToArray();

        private void AuthorizeActionAccess(EkApiRoleEnum apiRole, string actionName)
        {
            if (apiRole == EkApiRoleEnum.Admin)
            {
                return;
            }

            if (!EkActionNames.Contains(actionName))
            {
                throw new ActionAuthorizationException();
            }
        }

        private int ParseIdString(string idString)
        {
            if (string.IsNullOrEmpty(idString))
            {
                throw new ArgumentException("Id is empty.");
            }

            if (!int.TryParse(idString, out var id))
            {
                throw new ArgumentException("Id is not integer.");
            }

            return id;
        }

        private async Task<TRequest> ReadPostRequestAsync<TRequest>(HttpRequest httpRequest, IIntegrationLogManager integrationLogManager)
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

            const int maxRequestLoggingLength = 4_096;
            if (serializedRequest.Length > maxRequestLoggingLength)
            {
                integrationLogManager.LogToRequest("Big Request", $"length {serializedRequest.Length}");
                integrationLogManager.LogToRequest("Body Start", serializedRequest.Substring(0, maxRequestLoggingLength));
            }
            else
            {
                integrationLogManager.LogToRequest("Body", serializedRequest);
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
    }
}