using System.Text.Json.Serialization;

namespace DeploymentManager.Api.Infrastructure.ExternalServices.GitHub;

/// <summary>
/// GitHub release metadata.
/// </summary>
public sealed class GitHubReleaseDto
{
    public List<GitHubReleaseAssetDto> Assets { get; set; } = [];
}

/// <summary>
/// GitHub release asset metadata.
/// </summary>
public sealed class GitHubReleaseAssetDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [JsonPropertyName("browser_download_url")]
    public string BrowserDownloadUrl { get; set; } = string.Empty;
}