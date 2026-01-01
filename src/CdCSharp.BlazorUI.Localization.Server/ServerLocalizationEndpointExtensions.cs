namespace CdCSharp.BlazorUI.Localization.Server;

//public static class ServerLocalizationEndpointExtensions
//{
//    public static IEndpointRouteBuilder MapCultureEndpoint(this IEndpointRouteBuilder endpoints)
//    {
//        endpoints.MapGet("/Culture/Set", (string culture, string redirectUri, HttpContext httpContext) =>
//        {
//            if (!string.IsNullOrEmpty(culture))
//            {
//                // Aquí SÍ podemos establecer la cookie porque es una nueva solicitud HTTP
//                httpContext.Response.Cookies.Append(
//                    CookieRequestCultureProvider.DefaultCookieName,
//                    CookieRequestCultureProvider.MakeCookieValue(
//                        new RequestCulture(culture, culture)),
//                    new CookieOptions
//                    {
//                        Expires = DateTimeOffset.UtcNow.AddYears(1),
//                        IsEssential = true,
//                        SameSite = SameSiteMode.Lax,
//                        HttpOnly = true
//                    });
//            }

//            return Results.LocalRedirect(redirectUri);
//        })
//        .WithName("SetCulture")
//        .ExcludeFromDescription();

//        return endpoints;
//    }
//}
