using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace Havensread.Api.ErrorHandling;

internal sealed class ExceptionHandler
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(IHostEnvironment environment, ILogger<ExceptionHandler> logger)
    {
        _env = environment;
        _logger = logger;
    }

    public async Task HandleExceptionAsync(HttpContext httpContext)
    {
        httpContext.Response.ContentType = MediaTypeNames.Application.ProblemJson;

        var exHandlerFeature = httpContext.Features.Get<IExceptionHandlerFeature>()!;

        httpContext.RequestAborted.Register(async () =>
        {
            if (await HandleResponseAlreadyStartedAsync(httpContext, exHandlerFeature)) return;

            httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;

            await httpContext.Response.WriteAsJsonAsync(new ErrorResponse
            {
                StatusMessage = "Client Closed Request",
                Information = "The process was dropped due to a cancellation request."
            });
        });

        if (await HandleResponseAlreadyStartedAsync(httpContext, exHandlerFeature)) return;

        if (exHandlerFeature.Error is BadHttpRequestException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

            await httpContext.Response.WriteAsJsonAsync(new ErrorResponse
            {
                StatusMessage = "Bad Request",
                Information = "The request was malformed."
            });
            return;
        }

        if (exHandlerFeature.Error is DbUpdateConcurrencyException)
        {
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;

            await httpContext.Response.WriteAsJsonAsync(new ErrorResponse
            {
                StatusMessage = "Conflict",
                Information = "The resource was modified by another request."
            });
            return;
        }

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var errorResponse = _env.IsDevelopment() ? new DetailedErrorResponse
        {
            StatusMessage = "Internal Server Error",
            Information = "Unexpected error occured internally.",
            Reason = exHandlerFeature.Error.Message
        } : new ErrorResponse
        {
            StatusMessage = "Internal Server Error",
            Information = "Unexpected error occured internally.",
        };

        await httpContext.Response.WriteAsJsonAsync(errorResponse);

        var http = exHandlerFeature.Endpoint?.DisplayName?.Split(" => ")[0];
        var type = exHandlerFeature.Error.GetType().Name;
        var error = exHandlerFeature.Error.Message;

        const string Message = """
            ================================
            {Http}
            TYPE: {Type}
            REASON: {Error}
            INNER REASON: {Inner}
            ---------------------------------Outer
            {StackTrace},
            ---------------------------------Inner
            {InnerStrackTrace}
            ================================
            """;

        _logger.LogError(exHandlerFeature.Error, Message, http, type, error, exHandlerFeature.Error.StackTrace, exHandlerFeature.Error.InnerException?.Message, exHandlerFeature.Error.InnerException?.StackTrace);
    }

    private async Task<bool> HandleResponseAlreadyStartedAsync(HttpContext httpContext, IExceptionHandlerFeature exHandlerFeature)
    {
        if (httpContext.Response.HasStarted)
        {
            _logger.LogError(exHandlerFeature.Error, "The response has already started, the exception handler will not be executed.");

            try
            {
                await httpContext.Response.CompleteAsync();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "An error occured while completing the response.");
            }

            return true;
        }

        return false;
    }
}
