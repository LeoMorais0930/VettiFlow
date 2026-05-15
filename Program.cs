using VettiFlow.Api.Data;
using VettiFlow.Api.Endpoints;
using VettiFlow.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

// JsonStore — singleton que lê/grava vettiflow_data.json localmente
builder.Services.AddSingleton<JsonStore>();

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(opt =>
    opt.SerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.SetIsOriginAllowed(_ => true) // Permite qualquer origem (ideal para dev local/web)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

var app = builder.Build();

// Carrega os dados do arquivo JSON na subida
app.Services.GetRequiredService<JsonStore>().Load();

app.UseSwagger();
app.UseSwaggerUI(c => c.RoutePrefix = string.Empty); // Swagger na raiz "/"

app.UseCors();
app.MapHub<ProductionHub>("/hubs/production");
ApiEndpoints.Map(app);

app.Run("http://0.0.0.0:5000"); // Força escutar em todos os IPs da rede
