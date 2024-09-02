
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;
using Serilog;
using Microsoft.Extensions.Logging;
using TestWorkToWallet.DAL;
using TestWorkToWallet.WebSockets;
using TestWorkToWallet.Helpers;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()  // Логи выводятся в консоль
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)  // Логи записываются в файл
    .CreateLogger();

// Настройка приложения для использования Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllersWithViews();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Message Service API", Version = "v1" });
});


builder.Services.AddScoped<Helper>();

builder.Services.AddScoped<MessageRepository>(sp =>
    new MessageRepository(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sp.GetRequiredService<Helper>(),
        sp.GetRequiredService<ILogger<MessageRepository>>(),  // Передача логгера
        sp.GetRequiredService<WebSocketHandlerImplementation>()
    ));

// Register WebSocketHandler
builder.Services.AddSingleton<WebSocketHandlerImplementation>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Message Service API V1");
    });
}

app.UseWebSockets();
app.UseStaticFiles(); // Serve static files
app.UseMiddleware<WebSocketMiddleware>(); // Use WebSocket middleware

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Remove redundant WebSocket endpoint mapping
// The WebSocket endpoint should be handled by the middleware

app.Run();
