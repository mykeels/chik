using System.Collections.Generic;
using System.Net;
using System.Reflection;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Newtonsoft.Json;

namespace Chik.Exams.Api;

public static class GlobalExceptionHandlerExtensions
{
    internal static Dictionary<Type, HttpStatusCode> ExceptionToStatusCodeMapping =
        new Dictionary<Type, HttpStatusCode>();
    internal static bool ShouldIncludeExceptionDetailsInResponse =
        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != RemoteEnvironment.Production;
    internal static Func<object> GetExceptionData = () => new { };

    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var serverErrorService = context.RequestServices.GetRequiredService<IServerErrorService>();
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature?.Error;
                var logger = Provider.Logger;

                logger.LogWarning("Exception occurred");

                if (exception is null)
                {
                    logger.LogWarning("No exception found");
                    return;
                }

                if (exception is AggregateException aggregateException)
                {
                    logger.LogWarning("Aggregate exception occurred");
                    exception = aggregateException.InnerExceptions.FirstOrDefault();
                }

                var statusCode = GetHttpStatusCode(
                    context,
                    exception,
                    new Dictionary<Type, HttpStatusCode>()
                    {
                        { typeof(NotImplementedException), HttpStatusCode.NotImplemented },
                        { typeof(TimeoutException), HttpStatusCode.RequestTimeout },
                        { typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized },
                        { typeof(FusionAuthClientException), HttpStatusCode.Unauthorized },
                        {
                            typeof(System.ComponentModel.DataAnnotations.ValidationException),
                            HttpStatusCode.BadRequest
                        },
                        { typeof(ConflictException), HttpStatusCode.Conflict },
                        { typeof(KeyNotFoundException), HttpStatusCode.NotFound },
                    }
                        .Except(ExceptionToStatusCodeMapping, new ExceptionMappingComparer())
                        .Union(ExceptionToStatusCodeMapping)
                        .ToDictionary(kv => kv.Key, kv => kv.Value)
                ) ?? HttpStatusCode.InternalServerError;
                logger.LogWarning("Status code set to {statusCode}", statusCode);

                var controllerActionDescriptor = context
                    .GetEndpoint()
                    ?.Metadata.GetMetadata<ControllerActionDescriptor>();
                string? controller = controllerActionDescriptor?.ControllerName;
                string? action = controllerActionDescriptor?.ActionName;
                string operationId = context.Request.GetOperationId();
                string method = context.Request.Method;
                string path = context.Request.Path;

                logger.LogWarning("Controller: {controller}, Action: {action}, OperationId: {operationId}, Method: {method}, Path: {path}", controller, action, operationId, method, path);

                try
                {
                    var httpContextAccessor = context.RequestServices.GetRequiredService<IHttpContextAccessor>();
                    bool isAuthenticated = httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
                    var auth = isAuthenticated ? context.RequestServices.GetService<Auth>() : null;
                    var exceptionData = GetExceptionData?.Invoke();
                    
                    logger?.Error(
                        exception,
                        new
                        {
                            Message = $"Error in {controller}.{action}, Status: {statusCode}, Request: {path}, Method: {method}, Details: {exceptionData}",
                            Status = statusCode,
                            Controller = controller,
                            Action = action,
                            Request = new { Path = path, Method = method },
                            Application = Environment.GetEnvironmentVariable("APPSETTING_WEBSITE_SITE_NAME")
                                ?? Assembly.GetEntryAssembly()?.GetName().Name,
                            HostedIP = Environment.MachineName,
                            Auth = new
                            {
                                auth?.Id,
                                auth?.Username,
                            },
                            OperationId = operationId,
                            Data = exceptionData,
                        }
                    );
                    await serverErrorService.Create(new ServerError.Create(
                        OperationId: Guid.Parse(operationId),
                        Error: exception.Message.Truncate(512),
                        ErrorJson: JsonConvert.SerializeObject(new {
                            Exception = exception,
                            Data = exceptionData,
                            Controller = controller,
                            Action = action,
                            Request = new { Path = path, Method = method },
                            Auth = new
                            {
                                auth?.Id,
                                auth?.Username,
                            },
                        }),
                        UserId: auth?.Id,
                        RequestPath: path,
                        RequestMethod: method,
                        ErrorAt: DateTime.UtcNow
                    ));

                    var errorResponse = new
                    {
                        OperationId = operationId,
                        ErrorMessages = ShouldIncludeExceptionDetailsInResponse
                            ? GetExceptionMessages(exception)
                            : new List<string>
                            {
                                exception is IHumanReadableException
                                    ? exception.Message
                                    : "Exception Occured. Please contact support@under4.games",
                            },
                    };

                    // Clear auth cookies for unauthorized exceptions
                    if (exception is UnauthorizedAccessException or FusionAuthClientException)
                    {
                        var remoteEnvironment = context.RequestServices.GetRequiredService<RemoteEnvironment>();
                        httpContextAccessor.ClearAuthCookies(remoteEnvironment);
                    }

                    // Include the OperationId in the response Headers
                    context.Response.Headers.Append("X-Operation-Id", operationId);
                    context.Response.StatusCode = Convert.ToInt32(statusCode);
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(errorResponse);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Error writing response");
                    context.Response.StatusCode = Convert.ToInt32(statusCode);
                    context.Response.ContentType = "application/json";

                    await context.Response.WriteAsJsonAsync(new {
                        OperationId = operationId,
                        ErrorMessages = new List<string> { "Exception Occured. Please contact support@under4.games" },
                    });
                }
            });
        });
    }

    private static HttpStatusCode? GetHttpStatusCode(
        HttpContext context,
        Exception exception,
        Dictionary<Type, HttpStatusCode>? defaultMapping = null
    )
    {
        defaultMapping ??= new Dictionary<Type, HttpStatusCode>()
        {
            { typeof(UnauthorizedAccessException), HttpStatusCode.Unauthorized },
            {
                typeof(System.ComponentModel.DataAnnotations.ValidationException),
                HttpStatusCode.BadRequest
            },
        };
        
        var exceptionType = exception?.GetType();
        if (context.Items.ContainsKey(ExceptionExtensions.ErrorMappingKey))
        {
            var mapping = (Dictionary<Type, HttpStatusCode>?)
                context.Items[ExceptionExtensions.ErrorMappingKey];
            if (exceptionType != null && mapping != null && mapping.ContainsKey(exceptionType))
            {
                return mapping[exceptionType];
            }
            if (
                exceptionType != null
                && defaultMapping != null
                && defaultMapping.ContainsKey(exceptionType)
            )
            {
                return defaultMapping[exceptionType];
            }
            return null;
        }
        if (
            exceptionType != null
            && defaultMapping != null
            && defaultMapping.ContainsKey(exceptionType)
        )
        {
            return defaultMapping[exceptionType];
        }
        return null;
    }

    private static List<string> GetExceptionMessages(Exception exception)
    {
        var messages = new List<string>();
        while (exception is not null)
        {
            messages.Add(
#if NETFRAMEWORK
                exception is HttpResponseException
                    ? ((HttpResponseException)exception).Response.Content.ReadAsStringAsync().Result
                    :
#endif
                    exception.Message
            );
            messages.AddRange(
                (exception.StackTrace ?? "No StackTrace Found")
                    .Split('\n')
                    .Select(x => x.Trim())
                    .ToList()
            );
            exception = exception.InnerException!;
        }
        return messages;
    }
}

internal class ExceptionMappingComparer : IEqualityComparer<KeyValuePair<Type, HttpStatusCode>>
{
    public bool Equals(KeyValuePair<Type, HttpStatusCode> x, KeyValuePair<Type, HttpStatusCode> y)
    {
#pragma warning disable CA2013
        if (Object.ReferenceEquals(x, y))
            return true;
#pragma warning restore CA2013

#pragma warning disable CA2013
        if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
            return false;
#pragma warning restore CA2013

        return x.Key.FullName == y.Key.FullName;
    }

    public int GetHashCode(KeyValuePair<Type, HttpStatusCode> keyValue)
    {
#pragma warning disable CA2013
        if (Object.ReferenceEquals(keyValue, null))
            return 0;
#pragma warning restore CA2013

        if (keyValue.Key.FullName is null)
        {
            return keyValue.GetHashCode();
        }

        return keyValue.Key.FullName.GetHashCode();
    }
}

public interface IHumanReadableException { }

public static class ExceptionExtensions
{
    public const string ErrorMappingKey = "under4games.errorMap";

    public static string GetOperationId(this HttpContext context)
    {
        return context.Request.GetOperationId();
    }

    public static string GetOperationId(this HttpRequest request)
    {
        return request.Headers.TryGetValue("X-Operation-Id", out var operationId) 
            ? operationId.ToString() 
            : Guid.NewGuid().ToString();
    }
}