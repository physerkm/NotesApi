using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace NotesApi.ErrorHandling;

// Uygulamanın herhangi bir yerinde yakalanmamış bir exception olursa burası çalışır.
// Amaç: API çökmek yerine düzenli bir 500 yanıtı dönsün, hata da log'a yazılsın.
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService,
        IHostEnvironment environment)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // ProblemDetails yanıtındaki traceId ile aynı değer.
        // Kullanıcı bu kimliği bildirirse aynı hatayı log'da bulabiliriz.
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Ayrıntılar (mesaj + stack trace) sunucu tarafında log'a yazılır.
        _logger.LogError(exception, "Beklenmeyen hata: {Method} {Path} (traceId: {TraceId})",
            httpContext.Request.Method, httpContext.Request.Path, traceId);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Beklenmeyen bir hata oluştu.",
            // Teknik ayrıntıyı sadece geliştirme ortamında gösteriyoruz.
            // Canlı ortamda iç bilgiler (sınıf adları, dosya yolları) dışarı sızmamalı.
            Detail = _environment.IsDevelopment() ? exception.Message : null
        };

        // ProblemDetails servisi yanıta traceId'yi kendisi ekler.
        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}
