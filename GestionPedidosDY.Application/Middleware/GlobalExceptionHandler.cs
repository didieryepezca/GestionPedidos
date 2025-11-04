using GestionPedidosDY.Application.Middleware.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace GestionPedidosDY.Application.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger logger)
        {
            HttpResponse response = context.Response;
            response.ContentType = "application/json";

            logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

            var (statusCode, problemDetails) = exception switch
            {
                ServiceException serviceException => (serviceException.StatusCode, new ProblemDetails
                {
                    Status = serviceException.StatusCode,
                    Title = GetTitleForStatusCode(serviceException.StatusCode),
                    Detail = serviceException.Message
                }),
                _ => ((int)HttpStatusCode.InternalServerError, new ProblemDetails
                {
                    Status = (int)HttpStatusCode.InternalServerError,
                    Title = "Internal Server Error",
                    Detail = exception.Message
                })
            };

            response.StatusCode = statusCode;
            string result = JsonSerializer.Serialize(problemDetails);
            return response.WriteAsync(result);
        }

        private static string GetTitleForStatusCode(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad Request",
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status404NotFound => "Not Found",
                StatusCodes.Status409Conflict => "Conflict",
                StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
                StatusCodes.Status502BadGateway => "Bad Gateway",
                _ => "Error"
            };
        }
    }
}
