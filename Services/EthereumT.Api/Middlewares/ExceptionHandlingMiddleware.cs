using EthereumT.Api.Models.Dto;
using System.Text.Json;
using System.Net;

namespace EthereumT.Api.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                await HandleExceptionAsync(context, ex.Message, HttpStatusCode.InternalServerError, "Internal server error.");
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, string exceptionMessage, HttpStatusCode statusCode, string messageClient)
        {
            _logger.LogError(exceptionMessage);

            HttpResponse response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)statusCode;
            ErrorDto errorDto = new((int)statusCode, messageClient);
            string result = JsonSerializer.Serialize(errorDto);
            
            await response.WriteAsJsonAsync(result);
        }
    }
}