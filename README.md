# ASP.NET One-Time Token Authentication
Provides an authentication scheme for one-time access tokens to support file download via direct link for protected resources
## Frameworks supported
- ASP.NET Core 6.0
- ASP.NET Core 7.0
## Demo
A demo application can be found in folder `FS.Authentication.OneTimeToken.Demo`
## Installation
Install from NuGet package
```powershell
Install-Package Schick.Authentication.OneTimeToken
```
## Getting Started
Add `Authorize` attribute with one-time token authentication scheme to controller / minimal API
```csharp
// Authenticate via one-time access token.
[Authorize(AuthenticationSchemes = OneTimeTokenDefaults.AuthenticationScheme)]
internal static string RequireOneTimeTokenAuthentication()
	=> "Hello, you are authenticated";
```
Register one-time access token authorization scheme
```c#
// Add default authentication, e.g. NTLM/Windows, JWT, ...
builder.Services.AddAuthentication(...);
...
// Add one-time access token authentication.
builder.Services.AddOneTimeTokenAuthentication(config => { /* your options here */ });
```
Tokens can be generated via `IOneTimeTokenService.CreateToken`
```c#
<serviceProvider>.GetRequiredService<IOneTimeTokenService>().CreateToken(/* claims */);
```
## Full Sample
```c#
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

namespace FS.Authentication.OneTimeToken.Demo;

public class Program
{
    public const string DEFAULT_ROLE = "DefaultUser";

    // Authenticate / authorize via default authentication, e.g. NTLM/Windows, JWT, ...
    [Authorize]
    internal static string GetOneTimeToken(HttpContext httpContext, [FromQuery][DefaultValue(DEFAULT_ROLE)][SwaggerParameter(Required = false)] string role)
        => httpContext.RequestServices.GetRequiredService<IOneTimeTokenService>().CreateToken(role);

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
```