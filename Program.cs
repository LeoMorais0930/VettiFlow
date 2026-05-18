using VettiFlow.Api.Data;
using VettiFlow.Api.Endpoints;
using VettiFlow.Api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<JsonStore>();

builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureHttpJsonOptions(opt =>
    opt.SerializerOptions.ReferenceHandler =
        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
        p.SetIsOriginAllowed(_ => true) 
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()));

var app = builder.Build();

// Carrega os dados do arquivo JSON na subida
app.Services.GetRequiredService<JsonStore>().Load();

app.UseSwagger();
app.UseSwaggerUI(c => c.RoutePrefix = "swagger");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors();
app.MapHub<ProductionHub>("/hubs/production");
ApiEndpoints.Map(app);

// Resolve a usable local IPv4 address so mobile clients can connect using the machine IP
string GetLocalIPv4()
{
    try
    {
        foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up)
                continue;

            var ipProps = ni.GetIPProperties();
            foreach (var ua in ipProps.UnicastAddresses)
            {
                var ip = ua.Address;
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(ip))
                {
                    var bytes = ip.GetAddressBytes();
                    // skip APIPA addresses
                    if (bytes[0] == 169 && bytes[1] == 254) continue;
                    return ip.ToString();
                }
            }
        }
    }
    catch { }
    return "127.0.0.1";
}

var localIp = GetLocalIPv4();
var url = $"http://{localIp}:5000";
Console.WriteLine($"VettiFlow API listening on: {url}");
app.Run(url);
