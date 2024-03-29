using FS.Authentication.OneTimeToken.Abstractions.Interfaces;
using FS.Authentication.OneTimeToken.Abstractions.Models;
using FS.Authentication.OneTimeToken.Extensions;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel;
using System.Security.Claims;

namespace FS.Authentication.OneTimeToken.Demo;

public class Program
{
    public const string DEFAULT_ROLE = "DefaultRole";

    // Authenticate / authorize via default authentication, e.g. NTLM/Windows, JWT, ...
    [Authorize]
    internal static string GetOneTimeToken(HttpContext httpContext, [FromQuery][DefaultValue(DEFAULT_ROLE)][SwaggerParameter(Required = false)] string role)
        => httpContext.RequestServices.GetRequiredService<IOneTimeTokenService>().CreateToken(new Claim(ClaimTypes.Role, role));

    // Authenticate via one-time access token.
    [Authorize(AuthenticationSchemes = OneTimeTokenDefaults.AuthenticationScheme)]
    internal static string RequireOneTimeTokenAuthentication([FromQuery] string accessToken)
        => "Hello, you are authenticated";

    // Authenticate /authorize via one-time access token.
    [Authorize(AuthenticationSchemes = OneTimeTokenDefaults.AuthenticationScheme, Roles = DEFAULT_ROLE)]
    internal static string RequireOneTimeTokenAuthorization([FromQuery] string accessToken)
        => "Hello, you are authorized";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add default authentication, e.g. NTLM/Windows, JWT, ...
        builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();
        builder.Services.AddAuthorization();

        // Add one-time access token authentication.
        builder.Services.AddOneTimeTokenAuthentication(config => { /* your options here */ });

        AddSwagger(builder);

        var app = builder.Build();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet(nameof(GetOneTimeToken), GetOneTimeToken);
        app.MapGet(nameof(RequireOneTimeTokenAuthentication), RequireOneTimeTokenAuthentication);
        app.MapGet(nameof(RequireOneTimeTokenAuthorization), RequireOneTimeTokenAuthorization);

        AddSwaggerUi(app);

        app.Run();
    }

    private static void AddSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(config => config.EnableAnnotations());
    }

    private static void AddSwaggerUi(WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableTryItOutByDefault();
            options.ConfigObject.AdditionalItems.Add("requestSnippetsEnabled", true);
        });
    }
}