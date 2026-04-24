using DeploymentManager.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeploymentManager.Api.Application.Common.Interfaces;

/// <summary>
/// Defines the application's database context.
/// </summary>
public interface IDeploymentManagerDbContext
{
    DbSet<Agent> Agents { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Installation> Installations { get; }
    DbSet<Release> Releases { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}