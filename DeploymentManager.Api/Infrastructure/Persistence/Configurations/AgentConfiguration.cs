using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures agent entities.
/// </summary>
public sealed class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    /// <summary>
    /// Configures relationships and indexes for agent entities.
    /// </summary>
    public void Configure(EntityTypeBuilder<Agent> builder)
    {
        // One-to-many relationship where a customer can have multiple agents.
        // Disable cascade deletes to prevent customers from being deleted if they are linked to an agent.
        builder.HasOne(a => a.Customer)
            .WithMany()
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.Property(a => a.PublicId).IsRequired();
        builder.HasIndex(a => a.PublicId).IsUnique();

        builder.Property(a => a.Platform).IsRequired().HasMaxLength(50);
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1?view=efcore-10.0