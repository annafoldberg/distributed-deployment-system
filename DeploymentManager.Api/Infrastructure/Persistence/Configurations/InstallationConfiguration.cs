using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures installation entities.
/// </summary>
public sealed class InstallationConfiguration : IEntityTypeConfiguration<Installation>
{
    /// <summary>
    /// Configures relationships for installation entities.
    /// </summary>
    public void Configure(EntityTypeBuilder<Installation> builder)
    {
        // One-to-one relationship between installation and agent.
        // Disable cascade deletes to prevent an agent from being deleted if it is linked to an installation.
        builder.HasOne(i => i.Agent)
            .WithOne()
            .HasForeignKey<Installation>(i => i.AgentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.AgentId).IsUnique();

        // One-to-many relationship where multiple installations can share the same release.
        // Disable cascade deletes to prevent a release from being deleted if it is linked to an installation.
        builder.HasOne(i => i.Release)
            .WithMany()
            .HasForeignKey(i => i.ReleaseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1?view=efcore-10.0