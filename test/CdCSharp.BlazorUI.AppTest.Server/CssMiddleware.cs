using CdCSharp.BlazorUI.Core.Theming.Services;

namespace CdCSharp.BlazorUI.AppTest.Wasm;

public class CssMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IThemeService _cssService;

    public CssMiddleware(RequestDelegate next, IThemeService cssService)
    {
        _next = next;
        _cssService = cssService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path == "/blazor-ui/styles.css")
        {
            string css = _cssService.GenerateThemeCss();

            context.Response.ContentType = "text/css";
            context.Response.Headers["Cache-Control"] = "public, max-age=31536000";
            await context.Response.WriteAsync(css);
            return;
        }

        await _next(context);
    }
}
