using Autofac.Extras.FakeItEasy;
using FakeItEasy;
using FluentAssertions;
using FluentAssertions.Execution;
using FS.Authentication.OneTimeToken.Abstractions.Interfaces;
using FS.Authentication.OneTimeToken.Models;
using FS.Authentication.OneTimeToken.Services;
using FS.Authentication.OneTimeToken.Tests.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FS.Authentication.OneTimeToken.Tests;

[TestClass]
public class OneTimeTokenServiceTests
{
    [TestMethod]
    public void WhenTokenIsCreated_ResultIsNotEmpty()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();

        // Check
        token.Should().NotBeEmpty();
    }

    [TestMethod]
    public void WhenTokenIsCreated_ItIsValid()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();
        var validationResult = oneTimeTokenService.ValidateToken(token);

        // Check
        validationResult.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void WhenTokenWasValidated_ItBecomesInvalid()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();
        oneTimeTokenService.ValidateToken(token);
        var validationResult = oneTimeTokenService.ValidateToken(token);

        // Check
        validationResult.IsValid.Should().BeFalse();
    }

    [TestMethod]
    public void WhenTokenIsValidated_ClaimsAreReturned()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var claims = new[] { new Claim(ClaimTypes.Role, "RoleA ROLE"), new Claim(ClaimTypes.Role, "RoleB") };
        var token = oneTimeTokenService.CreateToken(claims);
        var validationResult = oneTimeTokenService.ValidateToken(token);

        // Check
        validationResult.Claims.Should().BeEquivalentTo(claims);
    }

    [TestMethod]
    public void WhenTokenIsExpired_ValidationFails()
    {
        // Prepare
        using var autoFake = CreateAutoFake(o => o.DefaultExpireTime = TimeSpan.Zero);
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken();
        var validationResult = oneTimeTokenService.ValidateToken(token);

        // Check
        validationResult.IsValid.Should().BeFalse();
    }

    [TestMethod]
    public void WhenNegativeExpiresInIsUsed_ArgumentOutOfRangeExceptionIsThrown()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        Action createToken = () => oneTimeTokenService.CreateToken(TimeSpan.MinValue);

        // Check
        createToken.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void WhenMaxExpiresInIsUsed_CreatedTokenIsValid()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var token = oneTimeTokenService.CreateToken(TimeSpan.MaxValue);
        var validationResult = oneTimeTokenService.ValidateToken(token);

        // Check
        validationResult.IsValid.Should().BeTrue();
    }

    [TestMethod]
    public void WhenInvalidTokenIsGiven_ValidationFails()
    {
        using var autoFake = CreateAutoFake(o => o.DefaultExpireTime = TimeSpan.Zero);
        var oneTimeTokenService = autoFake.Resolve<IOneTimeTokenService>();

        var validationResult = oneTimeTokenService.ValidateToken("N/A");
        validationResult.IsValid.Should().BeFalse();
    }

    [TestMethod]
    public async Task ExpiredTokens_AreRemovedFromStorage()
    {
        // Prepare
        using var autoFake = CreateAutoFake();
        var oneTimeTokenService = (OneTimeTokenService)autoFake.Resolve<IOneTimeTokenService>();

        // Act
        var validatedToken = oneTimeTokenService.CreateToken();
        var remainingToken = oneTimeTokenService.CreateToken();
        var shortToken = oneTimeTokenService.CreateToken(TimeSpan.FromMilliseconds(10));

        var rooTokens = oneTimeTokenService.Tokens.Clone(); // Create new reference
        await Task.Delay(15);
        oneTimeTokenService.ValidateToken(validatedToken); // Internally removes expired tokens.
        var remainingTokens = oneTimeTokenService.Tokens;

        // Check
        using var _ = new AssertionScope();
        rooTokens.Should().HaveCount(3);
        remainingTokens.Should().ContainSingle();
        remainingTokens.Single().Key.Should().Be(remainingToken);
    }

    private static AutoFake CreateAutoFake(Action<OneTimeTokenOptions> configureOptions = null)
    {
        var optionsMonitor = A.Fake<IOptionsMonitor<OneTimeTokenOptions>>();
        var options = new OneTimeTokenOptions();
        configureOptions?.Invoke(options);
        A.CallTo(() => optionsMonitor.CurrentValue).Returns(options);

        var autoFake = new AutoFake();
        autoFake.Provide(optionsMonitor);
        autoFake.Provide<IOneTimeTokenService, OneTimeTokenService>();
        return autoFake;
    }
}