using System.Security.Claims;
using System.Text.Encodings.Web;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Domain.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DeploymentManager.Api.Presentation.Authentication;

/// <summary>
/// Authenticates agent requests using the API key provided in the request header
/// and validates it against the stored hashed API key for the specified agent.
/// </summary>
public sealed class AgentApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Agent";
    private readonly IDeploymentManagerDbContext _dbContext;
    private readonly IPasswordHasher<Agent> _passwordHasher;

    public AgentApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IDeploymentManagerDbContext dbContext,
        IPasswordHasher<Agent> passwordHasher)
        : base(options, logger, encoder)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// Authenticates the request and creates an authenticated principal if validation succeeds. 
    /// </summary>
    /// <remarks>
    /// Expects API key in the "X-API-KEY" request header
    /// and agent identifier as a route value named "agentId".
    /// </remarks>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check API key header
        if (!Request.Headers.TryGetValue("X-API-KEY", out var apiKey))
        {
            Logger.LogWarning("Missing API key in request.");
            return AuthenticateResult.Fail("Missing API key.");
        }

        // Check agent's public ID from agentId route value
        if (!Request.RouteValues.TryGetValue("agentId", out var agentId) ||
            !Guid.TryParse(agentId?.ToString(), out var publicId))
        {
            Logger.LogWarning("Missing or invalid agentId route value in request.");
            return AuthenticateResult.Fail("Missing or invalid agentId route value.");
        }

        var agent = await _dbContext.Agents.FirstOrDefaultAsync(a => a.PublicId == publicId, Context.RequestAborted);
        
        if (agent is null)
        {
            Logger.LogWarning("Agent with PublicId {PublicId} not found.", publicId);
            return AuthenticateResult.Fail("Invalid agent.");
        }

        // Verify provided API key matches stored hash
        var verificationResult = _passwordHasher.VerifyHashedPassword(agent, agent.ApiKeyHash, apiKey.ToString());

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            Logger.LogWarning("Invalid API key for agent {PublicId}.", publicId);
            return AuthenticateResult.Fail("Invalid API key.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, agent.PublicId.ToString(), ClaimValueTypes.String, ClaimsIssuer),
            new Claim("agent_id", agent.PublicId.ToString(), ClaimValueTypes.String, ClaimsIssuer)
        };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return AuthenticateResult.Success(ticket);
    }
}

// Source:
// AuthenticationHandler: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationhandler-1?view=aspnetcore-10.0
// PasswordHasher: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1?view=aspnetcore-10.0
// Claims: https://learn.microsoft.com/en-us/dotnet/api/system.security.claims?view=net-10.0
// AuthenticationTicket: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.authenticationticket?view=aspnetcore-10.0