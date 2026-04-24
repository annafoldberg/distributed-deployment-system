using System.Net;
using System.Text;
using DeploymentManager.Api.Infrastructure.Configuration;
using DeploymentManager.Api.Infrastructure.ExternalServices.GitHub;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace DeploymentManager.Api.Tests.Infrastructure.ExternalServices.GitHub;

[TestClass]
public sealed class GitHubPackageProviderTests
{
    private Mock<HttpMessageHandler> _mockHttpHandler = null!;
    private HttpClient _httpClient = null!;
    private GitHubPackageProvider _provider = null!;
    private string _releasePath = "/releases";
    private string _downloadUri = "https://download-url/";

    [TestInitialize]
    public void TestInitialize()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://api.github.com/")
        };
        var options = Options.Create(new GitHubOptions
        {
            Owner = "owner",
            Repository = "repository"
        });
        var logger = Mock.Of<ILogger<GitHubPackageProvider>>();
        _provider = new GitHubPackageProvider(_httpClient, options, logger);
    }

    [TestMethod]
    public async Task FetchPackageAsync_MatchingAssetFound_Latest_ReturnsInstallationPackage()
    {
        // Arrange
        var platform = "osx-arm64";
        var version = "latest";

        var json = CreateReleaseMetadataJson(platform);
        SetupHttpMetadataResponse(json);
        SetupHttpDownloadResponse();

        // Act
        var result = await _provider.FetchPackageAsync(platform, version, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual($"app-{platform}.zip", result.FileName);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains($"{_releasePath}/latest")),
            ItExpr.IsAny<CancellationToken>());
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains($"{_releasePath}/tags/v{version}")),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task FetchPackageAsync_MatchingAssetFound_Tag_ReturnsInstallationPackage()
    {
        // Arrange
        var platform = "osx-arm64";
        var version = "1.0.0";

        var json = CreateReleaseMetadataJson(platform);
        SetupHttpMetadataResponse(json);
        SetupHttpDownloadResponse();

        // Act
        var result = await _provider.FetchPackageAsync(platform, version, CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual($"app-{platform}.zip", result.FileName);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains($"{_releasePath}/tags/v{version}")),
            ItExpr.IsAny<CancellationToken>());
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains($"{_releasePath}/latest")),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task FetchPackageAsync_PlatformIsWhiteSpace_ReturnsNull()
    {
        // Arrange
        var platform = " ";
        var version = "1.0.0";

        // Act
        var result = await _provider.FetchPackageAsync(platform, version, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task FetchPackageAsync_VersionIsWhiteSpace_ReturnsNull()
    {
        // Arrange
        var platform = "osx-arm64";
        var version = " ";

        // Act
        var result = await _provider.FetchPackageAsync(platform, version, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.IsAny<HttpRequestMessage>(),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task FetchPackageAsync_ReleaseNotFound_ReturnsNull()
    {
        // Arrange
        var platform = "osx-arm64";
        var version = "1.0.0";

        SetupHttpMetadataResponse(null, HttpStatusCode.NotFound);

        // Act
        var result = await _provider.FetchPackageAsync(platform, version, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(_releasePath)),
            ItExpr.IsAny<CancellationToken>());
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri == _downloadUri),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task FetchPackageAsync_NoMatchingAsset_ReturnsNull()
    {
        // Arrange
        var platform = "osx-arm64";
        var version = "1.0.0";

        var json = CreateReleaseMetadataJson("win-x64");
        SetupHttpMetadataResponse(json);

        // Act
        var result = await _provider.FetchPackageAsync(platform, version, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(_releasePath)),
            ItExpr.IsAny<CancellationToken>());
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Never(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri == _downloadUri),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task FetchPackageAsync_DownloadFails_ReturnsNull()
    {
        // Arrange
        var platform = "osx-arm64";
        var version = "1.0.0";

        var json = CreateReleaseMetadataJson(platform);
        SetupHttpMetadataResponse(json);
        SetupHttpDownloadResponse(HttpStatusCode.NotFound);

        // Act
        var result = await _provider.FetchPackageAsync(platform, version, CancellationToken.None);

        // Assert
        Assert.IsNull(result);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(_releasePath)),
            ItExpr.IsAny<CancellationToken>());
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri == _downloadUri),
            ItExpr.IsAny<CancellationToken>());
    }

    // -------------------- Helper Methods --------------------
    private string CreateReleaseMetadataJson(string platform)
    {
        return $$"""
        {
            "assets": [
                {
                    "browser_download_url": "{{_downloadUri}}",
                    "name": "app-{{platform}}.zip",
                    "content_type": "application/zip"
                }
            ]
        }
        """;
    }
    
    private void SetupHttpMetadataResponse(string? json, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _mockHttpHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(_releasePath)),
            ItExpr.IsAny<CancellationToken>()
        ).ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = statusCode == HttpStatusCode.OK ? new StringContent(json!, Encoding.UTF8, "application/json") : null
        });
    }

    private void SetupHttpDownloadResponse(HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _mockHttpHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri == _downloadUri),
            ItExpr.IsAny<CancellationToken>()
        ).ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = statusCode == HttpStatusCode.OK ? new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("download-content"))) : null
        });
    }
}

// Sources:
// Unit test best practices: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices
// GitHub Releases responses: https://docs.github.com/en/rest/releases/releases?apiVersion=2026-03-10
// Inspired by earlier mock HttpMessageHandler setup:
// https://github.com/annafoldberg/Team2_Gotorz/blob/master/Gotorz.Server.UnitTests/Services/FlightServiceTests.cs