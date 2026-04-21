using DeploymentManager.Api.Domain.Entities;
using DeploymentManager.Api.Infrastructure.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManager.Api.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds initial data into the database.
/// </summary>
public static class DeploymentManagerDbContextSeeder
{
    /// <summary>
    /// Ensures required entities exist in the database and creates them if missing.
    /// </summary>
    public static async Task SeedAsync(DbContext context, AgentSeedOptions agentSeedOptions, IPasswordHasher<Agent> passwordHasher, CancellationToken ct = default)
    {
        // Seed release
        var release = await context.Set<Release>().FirstOrDefaultAsync(r => r.Version == "1.0.0", ct);
        
        if (release is null)
        {
            release = new Release
            {
                Version = "1.0.0"
            };

            context.Set<Release>().Add(release);
        }

        // Seed customer
        var companyName = "Demo Company";
        var customer = await context.Set<Customer>().FirstOrDefaultAsync(c => c.CompanyName == companyName, ct);

        if (customer is null)
        {
            customer = new Customer
            {
                CompanyName = companyName,
                DesiredRelease = release
            };

            context.Set<Customer>().Add(customer);
        }

        // Seed agents        
        foreach (var seededAgent in agentSeedOptions.Agents)
        {
            var agentExists = await context.Set<Agent>().AnyAsync(a => a.PublicId == seededAgent.PublicId, ct);

            if (!agentExists)
            {
                var agent = new Agent
                {
                    PublicId = seededAgent.PublicId,
                    Platform = seededAgent.Platform,
                    Customer = customer
                };

                agent.ApiKeyHash = passwordHasher.HashPassword(agent, seededAgent.ApiKey);

                context.Set<Agent>().Add(agent);
            }
        }

        await context.SaveChangesAsync(ct);
    }
}

// Source:
// Seeding: https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding
// PasswordHasher: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.passwordhasher-1?view=aspnetcore-10.0