using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using MySqlManager;
using MySqlManager.Components;
using MySqlManager.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<SettingsService>();
builder.Services.AddSingleton<TopRowService>();
builder.Services.AddSingleton<OverlayService>();
builder.Services.AddSingleton<DatabaseConnectionService>();
builder.Services.AddSingleton<DatabaseInformationService>();
builder.Services.AddSingleton<TableInformationService>();
builder.Services.AddSingleton<SqlCommandService>();
builder.Services.AddSingleton<MySqlManagerService>();
builder.Services.AddSingleton<TruncateService>();

var freePort = Electron.GetAvailablePort(5000);
builder.WebHost.UseUrls($"http://localhost:{freePort}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var settingsService = app.Services.GetRequiredService<SettingsService>();
settingsService.Init();
var mysqlService = app.Services.GetRequiredService<MySqlManagerService>();
mysqlService.Init();

await app.StartAsync();

var mainProcessId = Process.GetCurrentProcess().Id;
Console.WriteLine("Own Process ID: " + Process.GetCurrentProcess().Id);

var server = app.Services.GetService<IServer>();
var addressFeature = server?.Features.Get<IServerAddressesFeature>();

var electronStarted = false;
foreach (var address in addressFeature?.Addresses!)
{
    Console.WriteLine("Server is listening on address: " + address);
    if (!electronStarted)
    {
        Task.Run(() =>
        {
            var electron = new Electron();
            electron.Start(address, mainProcessId);
        });

        electronStarted = true;
    }
}

await app.WaitForShutdownAsync();
