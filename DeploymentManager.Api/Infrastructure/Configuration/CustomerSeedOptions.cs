namespace DeploymentManager.Api.Infrastructure.Configuration;

/// <summary>
/// Seed configuration options for customer entity.
/// </summary>
public sealed class CustomerSeedOptions
{
    public const string SectionName = "CustomerSeeding";
    public Guid PublicId { get; init; }
    public string CompanyName { get; init; } = string.Empty;
}