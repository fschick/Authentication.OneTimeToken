using FS.Authentication.OneTimeToken.Abstractions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace FS.Authentication.OneTimeToken.Tests;

public class TestWebApplication
{
    public const string DEFAULT_RESULT = "Authenticated";
    public const string DEFAULT_ROLE = "SomeRole";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

        using var app = builder.Build();
        app.UseAuthorization();
        app.MapGet(nameof(AllowAnonymous), AllowAnonymous);
        app.MapGet(nameof(RequireAuthentication), RequireAuthentication);
        app.MapGet(nameof(RequireRole), RequireRole);
        app.MapGet(nameof(GetCurrentUserClaims), GetCurrentUserClaims);
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

    [Authorize(AuthenticationSchemes = OneTimeTokenDefaults.AuthenticationScheme)]
    internal static IEnumerable<Claim> GetCurrentUserClaims(HttpContext httpContext)
        => httpContext.User.Claims;
}