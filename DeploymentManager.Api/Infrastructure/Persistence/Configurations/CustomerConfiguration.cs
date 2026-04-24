using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures customer entities.
/// </summary>
public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <summary>
    /// Configures relationships and indexes for customer entities.
    /// </summary>
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // One-to-many relationship where multiple customers can share the same desired release.
        // Disable cascade deletes to prevent a release from being deleted if it is linked to a customer.
        builder.HasOne(c => c.DesiredRelease)
            .WithMany()
            .HasForeignKey(c => c.DesiredReleaseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.CompanyName).IsRequired().HasMaxLength(100);
        builder.HasIndex(c => c.CompanyName).IsUnique();
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1?view=efcore-10.0