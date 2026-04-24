using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures release entities.
/// </summary>
public sealed class ReleaseConfiguration : IEntityTypeConfiguration<Release>
{
    /// <summary>
    /// Configures indexes for release entities.
    /// </summary>
    public void Configure(EntityTypeBuilder<Release> builder)
    {
        builder.Property(r => r.Version).IsRequired().HasMaxLength(50);
        builder.HasIndex(r => r.Version).IsUnique();
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1?view=efcore-10.0