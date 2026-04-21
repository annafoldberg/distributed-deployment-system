using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManager.Api.Infrastructure.Persistence.Context;

/// <summary>
/// Entity Framework Core database context for the application.
/// </summary>
public sealed class DeploymentManagerDbContext : DbContext, IDeploymentManagerDbContext
{
    public DeploymentManagerDbContext(DbContextOptions<DeploymentManagerDbContext> options) : base(options) {}

    public DbSet<Agent> Agents { get; set; }

    public DbSet<Customer> Customers { get; set; }

    public DbSet<Installation> Installations { get; set; }

    public DbSet<Release> Releases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeploymentManagerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

// Sources:
// DbContext: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext?view=efcore-10.0
// ApplyConfigurationsFromAssembly: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.modelbuilder.applyconfigurationsfromassembly?view=efcore-10.0