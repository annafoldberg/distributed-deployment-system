using System.Security.Claims;
using System.Text.Encodings.Web;
using DeploymentManager.Api.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DeploymentManager.Api.Presentation.Authentication;

/// <summary>
/// Authenticates CLI requests using the API key provided in the request header
/// and validates it against the configured hashed API key.
/// </summary>
public sealed class CliApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Cli";
    private readonly CliIdentityOptions _options;
    private readonly IPasswordHasher<CliApiKeyAuthenticationHandler> _passwordHasher;

    public CliApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IOptions<CliIdentityOptions> cliOptions,
        IPasswordHasher<CliApiKeyAuthenticationHandler> passwordHasher)
        : base(options, logger, encoder)
    {
        _options = cliOptions.Value;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Authenticates the request and creates an authenticated principal if validation succeeds. 
    /// </summary>
    /// <remarks>
    /// Expects API key in the "X-API-KEY" request header.
    /// </remarks>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check API key header
        if (!Request.Headers.TryGetValue("X-API-KEY", out var apiKey))
            return Task.FromResult(AuthenticateResult.NoResult());

        // Verify provided API key matches stored hash
        var verificationResult = _passwordHasher.VerifyHashedPassword(this, _options.ApiKeyHash, apiKey.ToString());

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            Logger.LogWarning("Invalid CLI API key.");
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "cli", ClaimValueTypes.String, ClaimsIssuer),
            new Claim("client", "cli", ClaimValueTypes.String, ClaimsIssuer)
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

// Source:
// AuthenticationHandler: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationhandler-1?view=aspnetcore-10.0
// PasswordHasher: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1?view=aspnetcore-10.0
// Claims: https://learn.microsoft.com/en-us/dotnet/api/system.security.claims?view=net-10.0
// AuthenticationTicket: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationticket?view=aspnetcore-10.0