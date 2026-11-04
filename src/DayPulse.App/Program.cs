using DayPulse.App;
using DayPulse.App.Shared;
using DayPulse.Infrastructure;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();
builder.Services.AddInfrastructure();
builder.Services.AddDayPulseLocalization();
builder.Services.AddScoped<ThemeState>();
builder.Services.AddSingleton(TimeProvider.System);

var host = builder.Build();
_ = host.Services.GetRequiredService<CultureState>();
await host.RunAsync();
