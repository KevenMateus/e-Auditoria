using System.Net;
using System.Text.Json;

namespace EAuditoria.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, mensagem) = exception switch
        {
            KeyNotFoundException      => (HttpStatusCode.NotFound,            exception.Message),
            InvalidOperationException => (HttpStatusCode.UnprocessableEntity, exception.Message),
            ArgumentException         => (HttpStatusCode.BadRequest,          exception.Message),
            _                         => (HttpStatusCode.InternalServerError, "Ocorreu um erro interno. Tente novamente mais tarde.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Erro não tratado [{Type}]: {Message}\n{StackTrace}",
                exception.GetType().Name, exception.Message, exception.StackTrace);
        else
            _logger.LogWarning(exception, "Erro de negócio [{Type}]: {Message}",
                exception.GetType().Name, exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode  = (int)statusCode;

        object resposta = _env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError
            ? new
            {
                status   = (int)statusCode,
                mensagem = exception.Message,
                tipo     = exception.GetType().FullName,
                inner    = exception.InnerException?.Message,
                stack    = exception.StackTrace,
                traceId  = context.TraceIdentifier
            }
            : new
            {
                status   = (int)statusCode,
                mensagem,
                traceId  = context.TraceIdentifier
            };

        var json = JsonSerializer.Serialize(resposta, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
