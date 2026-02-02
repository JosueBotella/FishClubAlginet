var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddRadzenComponents();
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddAuthorizationCore();


var apiSettings = builder.Configuration.GetSection("ApiSettings").Get<ApiSettings>();

if (apiSettings is null || string.IsNullOrEmpty(apiSettings.BaseUrl))
{
    throw new InvalidOperationException(BlazorSettings.ApiSettingsSectionError);
}

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiSettings.BaseUrl)
});

builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();
await builder.Build().RunAsync();
