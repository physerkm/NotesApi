using NotesApi.Infrastructure;
using NotesApi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Servisler (Dependency Injection kayıtları) ---
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    // Swagger sayfasının en üstünde görünen başlık ve açıklama.
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "NotesApi";
        document.Info.Description =
            "Not uygulaması REST API'si. Notlar etiketlenebilir, etikete göre filtrelenebilir, " +
            "Türkçe alfabeye göre sıralanabilir ve silinen notlar geri alınabilir.";
        return Task.CompletedTask;
    });
});

// Hata yanıtlarını standart ProblemDetails (RFC 9457) biçiminde üretir.
builder.Services.AddProblemDetails();
// Yakalanmamış exception'ları GlobalExceptionHandler karşılar.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Bellekte tutulan notlar uygulama boyunca yaşasın diye Singleton.
builder.Services.AddSingleton<INoteService, InMemoryNoteService>();

var app = builder.Build();

// --- İstek hattı (middleware pipeline) ---
// En başta olmalı: kendinden sonra gelen her adımda oluşan hatayı yakalar.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // /openapi/v1.json
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "NotesApi v1");
        options.RoutePrefix = "swagger"; // http://localhost:5107/swagger
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
