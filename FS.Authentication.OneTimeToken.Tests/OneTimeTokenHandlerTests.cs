using Autofac.Extras.FakeItEasy;
using FluentAssertions;
using FluentAssertions.Execution;
using FS.Authentication.OneTimeToken.Abstractions.Interfaces;
using FS.Authentication.OneTimeToken.Abstractions.Models;
using FS.Authentication.OneTimeToken.Extensions;
using FS.Authentication.OneTimeToken.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FS.Authentication.OneTimeToken.Tests;

[TestClass]
public class OneTimeTokenHandlerTests
{
    [TestMethod]
    public async Task AllowAnonymousAction_SucceedsWithToken()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();

        // Act
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.AllowAnonymous), "INVALID TOKEN"));
        var returnValue = await response.Content.ReadAsStringAsync();

        // Check
        using var _ = new AssertionScope();
        response.IsSuccessStatusCode.Should().BeTrue();
        returnValue.Should().Be(TestWebApplication.DEFAULT_RESULT);
    }

    [TestMethod]
    public async Task AllowAnonymousAction_SucceedsWithoutToken()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();

        // Act
        var response = await httpClient.GetAsync($"{nameof(TestWebApplication.AllowAnonymous)}");
        var returnValue = await response.Content.ReadAsStringAsync();

        // Check
        using var _ = new AssertionScope();
        response.IsSuccessStatusCode.Should().BeTrue();
        returnValue.Should().Be(TestWebApplication.DEFAULT_RESULT);
    }

    [TestMethod]
    public async Task RequiredAuthenticationMethod_SucceedsWithValidQueryToken()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.RequireAuthentication), token));
        var returnValue = await response.Content.ReadAsStringAsync();

        // Check
        using var _ = new AssertionScope();
        response.IsSuccessStatusCode.Should().BeTrue();
        returnValue.Should().Be(TestWebApplication.DEFAULT_RESULT);
    }

    [TestMethod]
    public async Task RequiredAuthenticationMethod_SucceedsWithValidHeaderToken()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();
        httpClient.DefaultRequestHeaders.Add(OneTimeTokenDefaults.AuthorizationHeaderName, $"{OneTimeTokenDefaults.AuthorizationHeaderPrefix} {token}");
        var response = await httpClient.GetAsync(nameof(TestWebApplication.RequireAuthentication));
        var returnValue = await response.Content.ReadAsStringAsync();

        // Check
        using var _ = new AssertionScope();
        response.IsSuccessStatusCode.Should().BeTrue();
        returnValue.Should().Be(TestWebApplication.DEFAULT_RESULT);
    }

    [TestMethod]
    public async Task RequiredAuthenticationMethod_SucceedsWithCustomHeaderTokenConfiguration()
    {
        // Prepare
        var customerHeaderName = "customerHeaderName";
        var customerHeaderPrefix = "customerHeaderPrefix";
        using var autoFake = CreateAutoFake(o =>
        {
            o.AuthorizationHeaderName = customerHeaderName;
            o.AuthorizationHeaderPrefix = customerHeaderPrefix;
            o.Now = () => DateTime.UtcNow;
        });
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();
        httpClient.DefaultRequestHeaders.Add(customerHeaderName, $"{customerHeaderPrefix} {token}");
        var response = await httpClient.GetAsync(nameof(TestWebApplication.RequireAuthentication));
        var returnValue = await response.Content.ReadAsStringAsync();

        // Check
        using var _ = new AssertionScope();
        response.IsSuccessStatusCode.Should().BeTrue();
        returnValue.Should().Be(TestWebApplication.DEFAULT_RESULT);
    }

    [TestMethod]
    public async Task RequiredAuthenticationMethod_SucceedsWithCustomQueryTokenConfiguration()
    {
        // Prepare
        var queryParamName = "queryParamName";
        using var autoFake = CreateAutoFake(o => o.AuthorizationQueryParamName = queryParamName);
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.RequireAuthentication), token, queryParamName));
        var returnValue = await response.Content.ReadAsStringAsync();

        // Check
        using var _ = new AssertionScope();
        response.IsSuccessStatusCode.Should().BeTrue();
        returnValue.Should().Be(TestWebApplication.DEFAULT_RESULT);
    }

    [TestMethod]
    public async Task RequiredAuthenticationMethod_FailsWithInvalidToken()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();

        // Act
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.RequireAuthentication), "INVALID TOKEN"));

        // Check
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task RequiredAuthenticationMethod_FailsWithoutToken()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();

        // Act
        var response = await httpClient.GetAsync(nameof(TestWebApplication.RequireAuthentication));

        // Check
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task RequiredAuthenticationMethod_ReturnCustomNameIdentifier()
    {
        // Prepare
        using var autoFake = CreateAutoFake(o => o.NameIdentifier = "Some custom identifier");
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.GetCurrentUserClaims), token));
        var returnValue = await response.Content.ReadAsStringAsync();

        // Check
        returnValue.Should().Contain(@"""http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"",""value"":""Some custom identifier""");
    }

    [TestMethod]
    public async Task RequiredRoleMethod_SucceedsWithValidTokenAndRole()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken(TestWebApplication.DefaultRoleClaim);
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.RequireRole), token));
        var returnValue = await response.Content.ReadAsStringAsync();

        // Check
        using var _ = new AssertionScope();
        response.IsSuccessStatusCode.Should().BeTrue();
        returnValue.Should().Be(TestWebApplication.DEFAULT_RESULT);
    }

    [TestMethod]
    public async Task RequiredRoleMethod_FailsWithValidTokenButInvalidRole()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken(new Claim(ClaimTypes.Role, "INVALID ROLE"));
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.RequireRole), token));

        // Check
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [TestMethod]
    public async Task RequiredRoleMethod_FailsWithInvalidToken()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();

        // Act
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.RequireRole), "INVALID TOKEN"));

        // Check
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task RequiredRoleMethod_FailsWithoutToken()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();

        // Act
        var response = await httpClient.GetAsync(nameof(TestWebApplication.RequireRole));

        // Check
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [TestMethod]
    public async Task WhenClaimsSetToNull_TokenCanBeAuthenticated()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken((Claim[])null);
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.RequireAuthentication), token));

        // Check
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [TestMethod]
    public async Task WhenClaimIsNullNull_TokenCanBeAuthenticated()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var httpClient = autoFake.Resolve<HttpClient>();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken(new Claim[] { null });
        var response = await httpClient.GetAsync(WithToken(nameof(TestWebApplication.RequireAuthentication), token));

        // Check
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    private static string WithToken(string uri, string token, string tokenKey = OneTimeTokenDefaults.AuthorizationQueryParamName)
        => $"{uri}?{tokenKey}={token}";

    private static AutoFake CreateAutoFake(Action<OneTimeTokenOptions> configureAuthentication = null)
    {
        var autoFake = new AutoFake();
        var webApplication = new TestWebApplicationFactory(configureAuthentication);
        autoFake.Provide(webApplication);
        autoFake.Provide(webApplication.CreateClient());
        autoFake.Provide(webApplication.Server.Services.GetRequiredService<IOneTimeTokenService>());
        return autoFake;
    }

    private class TestWebApplicationFactory : WebApplicationFactory<TestWebApplication>
    {
        private readonly Action<OneTimeTokenOptions> _configureAuthentication;
        public TestWebApplicationFactory(Action<OneTimeTokenOptions> configureAuthentication = null)
            => _configureAuthentication = configureAuthentication ?? (_ => { });

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services =>
            {
                services.Configure(_configureAuthentication);
                services.AddOneTimeTokenAuthentication();
            });
        }
    }
}