using NotesApi.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Servisler (Dependency Injection kayıtları) ---
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Bellekte tutulan notlar uygulama boyunca yaşasın diye Singleton.
builder.Services.AddSingleton<INoteService, InMemoryNoteService>();

var app = builder.Build();

// --- İstek hattı (middleware pipeline) ---
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
