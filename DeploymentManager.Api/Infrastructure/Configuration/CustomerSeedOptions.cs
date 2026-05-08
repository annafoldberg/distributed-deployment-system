namespace DeploymentManager.Api.Infrastructure.Configuration;

/// <summary>
/// Seed configuration options for customer entities.
/// </summary>
public sealed class CustomerSeedOptions
{
    public const string SectionName = "CustomerSeeding";
    public List<SeededCustomer> Customers { get; init; } = [];
}

public sealed class SeededCustomer
{
    public Guid PublicId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
}