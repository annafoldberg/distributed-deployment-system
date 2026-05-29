using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configures audit log entities.
/// </summary>
public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    /// <summary>
    /// Configures relationships and properties for audit log entities.
    /// </summary>
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // One-to-many relationship where a customer can have multiple audit logs.
        builder.HasOne(l => l.Customer)
            .WithMany()
            .HasForeignKey(l => l.CustomerId)
            .OnDelete(DeleteBehavior.NoAction);
        
        // One-to-many relationship where an agent can have multiple audit logs.
        builder.HasOne(l => l.Agent)
            .WithMany()
            .HasForeignKey(l => l.AgentId)
            .OnDelete(DeleteBehavior.NoAction);
            
        builder.Property(l => l.CreatedAt)
            .IsRequired();
        
        builder.Property(l => l.Level)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);
        
        builder.Property(l => l.Message)
            .IsRequired()
            .HasMaxLength(200);
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1?view=efcore-10.0