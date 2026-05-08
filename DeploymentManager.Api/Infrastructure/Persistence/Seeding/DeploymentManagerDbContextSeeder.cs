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
    public static async Task SeedAsync(
        DbContext context,
        CustomerSeedOptions customerSeedOptions,
        AgentSeedOptions agentSeedOptions,
        ReleaseSeedOptions releaseSeedOptions,
        IPasswordHasher<Agent> passwordHasher,
        CancellationToken ct = default)
    {
        // Seed releases
        foreach (var seededRelease in releaseSeedOptions.Releases)
        {
            var releaseExists = await context.Set<Release>().AnyAsync(r => r.Version == seededRelease.Version, ct);

            if (!releaseExists)
            {
                var release = new Release
                {
                    Version = seededRelease.Version
                };

                context.Set<Release>().Add(release);
            }
        }
        
        // Seed customers
        foreach (var seededCustomer in customerSeedOptions.Customers)
        {
            var customerExists = await context.Set<Customer>().AnyAsync(c => c.PublicId == seededCustomer.PublicId, ct);

            if (!customerExists)
            {
                var customer = new Customer
                {
                    PublicId = seededCustomer.PublicId,
                    CompanyName = seededCustomer.CompanyName
                };

                context.Set<Customer>().Add(customer);
            }
        }

        await context.SaveChangesAsync(ct);

        // Seed agents        
        foreach (var seededAgent in agentSeedOptions.Agents)
        {
            var agentExists = await context.Set<Agent>().AnyAsync(a => a.PublicId == seededAgent.PublicId, ct);

            if (!agentExists)
            {
                var customer = await context.Set<Customer>()
                    .FirstOrDefaultAsync(c => c.CompanyName == seededAgent.CompanyName, ct);
                    
                if (customer is null)
                    throw new InvalidOperationException($"Customer {seededAgent.CompanyName} not found.");

                var agent = new Agent
                {
                    PublicId = seededAgent.PublicId,
                    Platform = seededAgent.Platform,
                    CustomerId = customer.Id
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