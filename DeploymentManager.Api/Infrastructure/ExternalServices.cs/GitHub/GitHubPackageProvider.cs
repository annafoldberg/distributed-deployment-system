using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DeploymentManager.Api.Application.Features.InstallationPackages.Interfaces;
using DeploymentManager.Api.Application.Features.InstallationPackages.Models;

namespace DeploymentManager.Api.Infrastructure.ExternalServices.GitHub;

/// <summary>
/// Retrieves installation packages from GitHub Releases.
/// </summary>
public class GitHubPackageProvider : IPackageProvider
{
    private readonly HttpClient _httpClient;
    private readonly GitHubOptions _options;
    private readonly string _releasesPath;
    private readonly ILogger<GitHubPackageProvider> _logger;

    public GitHubPackageProvider(
        HttpClient httpClient,
        IOptions<GitHubOptions> options,
        ILogger<GitHubPackageProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _releasesPath = $"repos/{_options.Owner}/{_options.Repository}/releases";
        _logger = logger;
    }

    public async Task<InstallationPackage?> FetchPackageAsync(string platform, string version, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(platform) || string.IsNullOrWhiteSpace(version))
        {
            _logger.LogWarning("Package fetch aborted because platform or version was empty");
            return null;
        }

        // Get release metadata
        GitHubReleaseDto? release =
            version == "latest"
                ? await GetReleaseLatestAsync(ct)
                : await GetReleaseByTagAsync(version, ct);

        if (release == null) return null;

        // Select matching asset
        var asset = release.Assets.FirstOrDefault(a =>
            a.Name.Contains(platform, StringComparison.OrdinalIgnoreCase) &&
            a.Name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase));

        if (asset == null)
        {
            _logger.LogWarning(
                "No matching asset found for platform {Platform} and version {Version}",
                platform,
                version);
            return null;
        }

        // Download asset content
        var content = await DownloadAssetAsync(asset.BrowserDownloadUrl, ct);
        if (content == null) return null;

        return new InstallationPackage(content, asset.ContentType, asset.Name);
    }

    /// <summary>
    /// Retrieves release metadata for the specified version.
    /// </summary>
    /// <returns>The release metadata if found, otherwise <c>null</c>.</returns>
    private async Task<GitHubReleaseDto?> GetReleaseByTagAsync(string version, CancellationToken ct)
    {
        var tag = $"v{version}";
        var response = await _httpClient.GetAsync($"{_releasesPath}/tags/{tag}", ct);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not retrieve GitHub release for version {Version}", version);
            return null;
        }
        return await response.Content.ReadFromJsonAsync<GitHubReleaseDto>(ct);        
    }

    /// <summary>
    /// Retrieves metadata for the latest release.
    /// </summary>
    /// <returns>The release metadata if found, otherwise <c>null</c>.</returns>
    private async Task<GitHubReleaseDto?> GetReleaseLatestAsync(CancellationToken ct)
    {
        var response = await _httpClient.GetAsync($"{_releasesPath}/latest", ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not retrieve latest GitHub release");
            return null;
        }
        return await response.Content.ReadFromJsonAsync<GitHubReleaseDto>(ct);
    }

    private async Task<Stream?> DownloadAssetAsync(string downloadUrl, CancellationToken ct)
    {
        // ResponseHeadersRead avoids buffering large responses into memory
        var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "GitHub asset download failed for {DownloadUrl} with status code {StatusCode}",
                downloadUrl,
                response.StatusCode);
            return null;
        }
        return await response.Content.ReadAsStreamAsync(ct);
    }
}

// Sources:
// HTTP requests: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-10.0
// GitHub Releases API: https://docs.github.com/en/rest/releases/releases?apiVersion=2026-03-10