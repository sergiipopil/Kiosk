using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using KioskBrains.Common.Contracts;
using KioskBrains.Waf.Actions.Common;
using KioskBrains.Waf.Helpers.Exceptions;
using KioskBrains.Waf.Helpers.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskBrains.Waf.Actions.Processing.Web
{
    internal class WafActionWebProcessor
    {
        private readonly WafActionProcessor _wafActionProcessor;

        public WafActionWebProcessor(WafActionProcessor wafActionProcessor)
        {
            Assure.ArgumentNotNull(wafActionProcessor, nameof(wafActionProcessor));

            _wafActionProcessor = wafActionProcessor;
        }

        public void AddRouting(IServiceCollection services)
        {
            // uses standard routing
            services.AddRouting();
        }


        public void AddWafApiRoutes(IApplicationBuilder applicationBuilder)
        {
            PrepareWafActions();

            var routeHandler = new RouteHandler(ProcessWafActionRequestAsync);
            var routeBuilder = new RouteBuilder(applicationBuilder, routeHandler);
            routeBuilder.MapRoute("waf", WafConstants.WafRouteTemplatePath);
            applicationBuilder.UseRouter(routeBuilder.Build());
        }

        internal static Type GetWafActionType(string requestMethod, string actionKebabName)
        {
            var methodActions = _wafActionsByMethodAndKebabCaseName.GetValueOrDefault(requestMethod);
            var actionWebMetadata = methodActions.GetValueOrDefault(actionKebabName);
            return actionWebMetadata?.ActionType;
        }

        private async Task ProcessWafActionRequestAsync(HttpContext context)
        {
            Assure.ArgumentNotNull(context, nameof(context));

            var logger = context.RequestServices.GetRequiredService<ILogger<WafActionWebProcessor>>();

            var response = await ProcessAndExecuteWafActionRequestAsync(context, logger);

            // make sure the response is well-formed
            if (response == null)
            {
                response = GetErrorResponse(logger, WafActionResponseCodeEnum.InternalServerError, "Empty response.");
            }
            else if (response.Meta == null)
            {
                response.Meta = new WafActionResponseMeta(WafActionResponseCodeEnum.InternalServerError, "Empty response meta.");
            }

            // response serialization
            string responseBody = null;
            if (response.Meta.Code == WafActionResponseCodeEnum.Ok)
            {
                try
                {
                    var serializerSettings = JsonConvert.DefaultSettings();
                    // try to serialize response with data
                    if (context.Request.Headers.Keys.Contains("X-Api-Request"))
                    {
                        serializerSettings.TypeNameHandling = TypeNameHandling.Arrays;
                    }

                    responseBody = JsonConvert.SerializeObject(response, serializerSettings);
                }
                catch (Exception ex)
                {
                    response.Meta.Code = WafActionResponseCodeEnum.InternalServerError;
                    response.Meta.ErrorMessage = $"Response data serialization error: '{ex.Message}'.";
                }
            }

            // serialize error response
            if (responseBody == null)
            {
                responseBody = JsonConvert.SerializeObject(response);
            }

            // send response
            var httpResponse = context.Response;
            httpResponse.StatusCode = (int)response.Meta.Code;
            httpResponse.ContentType = "application/json";
            await httpResponse.WriteAsync(responseBody);
        }

        private async Task<WafActionResponseWrapper> ProcessAndExecuteWafActionRequestAsync(HttpContext context, ILogger logger)
        {
            Assure.ArgumentNotNull(context, nameof(context));

            try
            {
                // find action
                var httpRequest = context.Request;
                var methodActions = _wafActionsByMethodAndKebabCaseName.GetValueOrDefault(httpRequest.Method);
                if (methodActions == null)
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.MethodNotAllowed, $"Method '{httpRequest.Method}' is not supported.");
                }

                var routingFeature = (IRoutingFeature)context.Features[typeof(IRoutingFeature)];
                if (routingFeature == null)
                {
                    throw new InvalidOperationException($"'{nameof(IRoutingFeature)}' is not set.");
                }

                var actionKebabName = (string)routingFeature.RouteData.Values["action"];
                var extraPath = (string)routingFeature.RouteData.Values["extra"];

                if (!string.IsNullOrEmpty(extraPath))
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.BadRequest, $"Unexpected action path extension '{extraPath}'. Only requests in format '{WafConstants.WafApiUrlRoot}/{{action-name}}' are supported.");
                }

                var actionWebMetadata = methodActions.GetValueOrDefault(actionKebabName);
                if (actionWebMetadata == null)
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.BadRequest, $"Action '{actionKebabName}' is not supported.");
                }

                // get serialized request
                string serializedRequest;
                if (httpRequest.Method == "GET" || httpRequest.Method == "DELETE")
                {
                    // todo: build request from query as regular MVC binders do?
                    serializedRequest = httpRequest.Query[WafConstants.WafApiGetRequestParameterName];
                    if (serializedRequest == null)
                    {
                        return GetErrorResponse(logger, WafActionResponseCodeEnum.BadRequest, $"Request is not passed. For GET: request's JSON should be passed via URL parameter '{WafConstants.WafApiGetRequestParameterName}'.");
                    }
                }
                else
                {
                    // no need to rewind request since at this point WAF is a final handler
                    using (var streamReader = new StreamReader(httpRequest.Body, Encoding.UTF8))
                    {
                        serializedRequest = await streamReader.ReadToEndAsync();
                    }

                    if (serializedRequest == null)
                    {
                        return GetErrorResponse(logger, WafActionResponseCodeEnum.BadRequest, $"Request is not passed. For {httpRequest.Method}: request's JSON should be passed via HTTP body.");
                    }
                }

                // deserialize request
                object requestObject;
                try
                {
                    requestObject = JsonConvert.DeserializeObject(serializedRequest, actionWebMetadata.RequestType);
                    if (requestObject == null)
                    {
                        return GetErrorResponse(logger, WafActionResponseCodeEnum.BadRequest, "Empty requests are not supported.");
                    }
                }
                catch (Exception ex)
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.BadRequest, $"Invalid request structure: '{ex.Message}'.");
                }

                // execute action
                try
                {
                    var responseData = await _wafActionProcessor.ExecuteActionAsync(context.RequestServices, actionWebMetadata.ActionType, requestObject);
                    return new WafActionResponseWrapper()
                        {
                            Meta = new WafActionResponseMeta(WafActionResponseCodeEnum.Ok),
                            Data = responseData,
                        };
                }
                catch (OperationCanceledException)
                {
                    // request cancelled
                    return new WafActionResponseWrapper()
                        {
                            Meta = new WafActionResponseMeta(WafActionResponseCodeEnum.BadRequest, "Cancelled."),
                        };
                }
                // todo: add specific exception types (validation, entity not found, etc.)
                catch (AuthenticationRequiredException ex)
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.Unauthorized, ex.Message, skipLogging: true);
                }
                catch (ActionAuthorizationException ex)
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.Forbidden, ex.Message);
                }
                catch (DataAuthorizationException ex)
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.Forbidden, ex);
                }
                catch (AuthenticationException ex)
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.Forbidden, ex);
                }
                catch (Exception ex)
                {
                    return GetErrorResponse(logger, WafActionResponseCodeEnum.InternalServerError, $"Action execution failed: '{ex.Message}'.", ex);
                }
            }
            // general catch
            catch (Exception ex)
            {
                return GetErrorResponse(logger, WafActionResponseCodeEnum.InternalServerError, ex);
            }
        }

        public WafActionResponseWrapper GetErrorResponse(
            ILogger logger,
            WafActionResponseCodeEnum code,
            Exception exception,
            bool skipLogging = false)
        {
            return GetErrorResponse(logger, code, exception?.Message, exception, skipLogging);
        }

        public WafActionResponseWrapper GetErrorResponse(
            ILogger logger,
            WafActionResponseCodeEnum code,
            string errorMessage,
            Exception exception = null,
            bool skipLogging = false)
        {
            if (!skipLogging)
            {
                if (exception == null)
                {
                    logger.LogError((int)code, errorMessage);
                }
                else
                {
                    logger.LogError((int)code, exception, errorMessage);
                }
            }

            return new WafActionResponseWrapper()
                {
                    Meta = new WafActionResponseMeta(code, errorMessage),
                };
        }

        private static Dictionary<string, Dictionary<string, WafActionWebMetadata>> _wafActionsByMethodAndKebabCaseName;

        private void PrepareWafActions()
        {
            // prepare methods
            _wafActionsByMethodAndKebabCaseName = new Dictionary<string, Dictionary<string, WafActionWebMetadata>>();
            foreach (var supportedActionNameMethod in WafActionNameHelper.SupportedActionNameMethods)
            {
                _wafActionsByMethodAndKebabCaseName[supportedActionNameMethod.ToUpper()] = new Dictionary<string, WafActionWebMetadata>();
            }

            // link the actions by kebab case name
            var requestTypeInternalPropertyName = nameof(WafAction<object, object>.RequestTypeInternal);
            var actionTypes = _wafActionProcessor.WafActionTypes;
            foreach (var actionTypeItem in actionTypes)
            {
                var actionType = actionTypeItem.Key;
                var actionMetadata = actionTypeItem.Value;

                // get request type
                var requestTypeProperty = actionType.GetProperty(requestTypeInternalPropertyName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                var requestType = (Type)requestTypeProperty.GetValue(actionType);

                var methodActions = _wafActionsByMethodAndKebabCaseName[actionMetadata.WafActionName.Method];
                methodActions[actionMetadata.WafActionName.Name.PascalToKebabCase()] = new WafActionWebMetadata()
                    {
                        ActionType = actionType,
                        RequestType = requestType,
                    };
            }
        }
    }
}