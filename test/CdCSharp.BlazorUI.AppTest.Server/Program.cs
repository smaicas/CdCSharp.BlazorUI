using CdCSharp.BlazorUI.AppTest.Server.Components;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorUI();

builder.Services.AddCdCSharpBlazorUILocalization(options =>
{
    options.SupportedCultures =
    [
        new CultureInfo("en-US"),
        new CultureInfo("es-ES"),
        new CultureInfo("fr-FR")
    ];
    options.DefaultCulture = "en-US";
    options.CultureCookieName = ".AspNetCore.Culture"; // Cookie para persistir la cultura
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Add localization middleware - IMPORTANTE: debe ir antes de UseAntiforgery y MapRazorComponents
app.UseCdCSharpBlazorUILocalization();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapGet("/Culture/Set", (string culture, string redirectUri, HttpContext httpContext) =>
        {
            if (!string.IsNullOrEmpty(culture))
            {
                // Aquí SÍ podemos establecer la cookie porque es una nueva solicitud HTTP
                httpContext.Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(
                        new RequestCulture(culture, culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        HttpOnly = true
                    });
            }

            return Results.LocalRedirect(redirectUri);
        })
        .WithName("SetCulture")
        .ExcludeFromDescription();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();