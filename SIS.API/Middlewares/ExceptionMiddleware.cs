using SIS.BLL.Exceptions;
using System.Net;
using System.Text.Json;

namespace SIS.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Xəta baş verdi: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            int statusCode;
            object response;

            switch (exception)
            {
                case FluentValidation.ValidationException validationEx:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        StatusCode = statusCode,
                        Message = "Validation xətası",
                        Errors = validationEx.Errors
                            .Select(e => new { Field = e.PropertyName, Error = e.ErrorMessage })
                            .ToList(),
                        Timestamp = DateTime.UtcNow
                    };
                    break;

                case AppException appEx:
                    statusCode = (int)appEx.StatusCode;
                    response = new
                    {
                        StatusCode = statusCode,
                        Message = appEx.Message,
                        Timestamp = DateTime.UtcNow
                    };
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    response = new
                    {
                        StatusCode = statusCode,
                        Message = "Xəta baş verdi",
                        Timestamp = DateTime.UtcNow
                    };
                    break;
            }

            context.Response.StatusCode = statusCode;

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
