using FS.Authentication.OneTimeToken.Abstractions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FS.Authentication.OneTimeToken.Tests;

public class TestWebApplication
{
    public const string DEFAULT_RESULT = "Authenticated";
    public const string DEFAULT_ROLE = "SomeRole";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        using var app = builder.Build();
        app.UseAuthorization();
        app.MapGet(nameof(AllowAnonymous), AllowAnonymous);
        app.MapGet(nameof(RequireAuthentication), RequireAuthentication);
        app.MapGet(nameof(RequireRole), RequireRole);
        app.Run();
    }

    [AllowAnonymous]
    internal static string AllowAnonymous(HttpContext httpContext)
        => DEFAULT_RESULT;

    [Authorize(AuthenticationSchemes = OneTimeTokenDefaults.AuthenticationScheme)]
    internal static string RequireAuthentication(HttpContext httpContext)
        => DEFAULT_RESULT;

    [Authorize(AuthenticationSchemes = OneTimeTokenDefaults.AuthenticationScheme, Roles = DEFAULT_ROLE)]
    internal static string RequireRole(HttpContext httpContext)
        => DEFAULT_RESULT;
}