using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DeploymentManager.Agent.Infrastructure.Api;
using DeploymentManager.Agent.Infrastructure.Configuration;
using System.Net;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DeploymentManager.Agent.Application.Features.Deployment.Results;

namespace DeploymentManager.Agent.Tests.Infrastructure.Api;

[TestClass]
public sealed class DeploymentManagerApiClientTests
{
    private Mock<HttpMessageHandler> _mockHttpHandler = null!;
    private HttpClient _httpClient = null!;
    private DeploymentManagerApiClient _client = null!;
    private Guid _agentId;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockHttpHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("https://deployment-manager.local/api/")
        };
        _agentId = Guid.NewGuid();
        var options = Options.Create(new AgentIdentityOptions
        {
            AgentId = _agentId,
            ApiKey = "test-api-key"
        });
        var logger = Mock.Of<ILogger<DeploymentManagerApiClient>>();
        _client = new DeploymentManagerApiClient(_httpClient, options, logger);
    }

    // -------------------- GetInstallationPackageAsync --------------------
    [TestMethod]
    public async Task GetInstallationPackageAsync_IsValidResponseWithPackage_ReturnsInstallationPackage()
    {
        // Arrange
        var requestUri = $"deployments/{_agentId}/package";
        SetupGetInstallationPackageHttpResponse(requestUri, releaseVersion: "1.0.0", fileNameStar: "app-osx-arm64.zip");

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(PackageRetrievalStatus.UpdateAvailable, result.Status);
        Assert.IsNotNull(result.InstallationPackage);
        Assert.IsNotNull(result.InstallationPackage.Content);
        Assert.AreEqual("1.0.0", result.InstallationPackage.Version);
        Assert.AreEqual("app-osx-arm64.zip", result.InstallationPackage.FileName);
        var content = await new StreamReader(result.InstallationPackage.Content).ReadToEndAsync();
        Assert.AreEqual("package-content", content);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_FileNameStarIsPresent_UsesFileNameStar()
    {
        // Arrange
        var requestUri = $"deployments/{_agentId}/package";
        SetupGetInstallationPackageHttpResponse(requestUri, releaseVersion: "1.0.0",
            fileNameStar: "app-osx-arm64.zip", fileName: "app-win-x64.zip");

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(PackageRetrievalStatus.UpdateAvailable, result.Status);
        Assert.IsNotNull(result.InstallationPackage);
        Assert.IsNotNull(result.InstallationPackage.Content);
        Assert.AreEqual("1.0.0", result.InstallationPackage.Version);
        Assert.AreEqual("app-osx-arm64.zip", result.InstallationPackage.FileName);
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_FileNameStarIsNotPresent_UsesFileName()
    {
        // Arrange
        var requestUri = $"deployments/{_agentId}/package";
        SetupGetInstallationPackageHttpResponse(requestUri, releaseVersion: "1.0.0", fileName: "app-osx-arm64.zip");

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(PackageRetrievalStatus.UpdateAvailable, result.Status);
        Assert.IsNotNull(result.InstallationPackage);
        Assert.IsNotNull(result.InstallationPackage.Content);
        Assert.AreEqual("1.0.0", result.InstallationPackage.Version);
        Assert.AreEqual("app-osx-arm64.zip", result.InstallationPackage.FileName);
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_IsNotSuccessStatusCode_ReturnsFailed()
    {
        // Arrange
        var requestUri = $"deployments/{_agentId}/package";
        SetupGetInstallationPackageHttpResponse(requestUri, HttpStatusCode.NotFound);

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(PackageRetrievalStatus.Failed, result.Status);
        Assert.IsNull(result.InstallationPackage);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_NoContentStatusCode_ReturnsNoUpdateRequired()
    {
        // Arrange
        var requestUri = $"deployments/{_agentId}/package";
        SetupGetInstallationPackageHttpResponse(requestUri, HttpStatusCode.NoContent);

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(PackageRetrievalStatus.NoUpdateRequired, result.Status);
        Assert.IsNull(result.InstallationPackage);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_VersionMissing_ReturnsFailed()
    {
        // Arrange
        var requestUri = $"deployments/{_agentId}/package";
        SetupGetInstallationPackageHttpResponse(requestUri, fileNameStar: "app-osx-arm64.zip", fileName: "app-win-x64.zip");

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(PackageRetrievalStatus.Failed, result.Status);
        Assert.IsNull(result.InstallationPackage);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_FileNameMissing_ReturnsFailed()
    {
        // Arrange
        var requestUri = $"deployments/{_agentId}/package";
        SetupGetInstallationPackageHttpResponse(requestUri, releaseVersion: "1.0.0");

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(PackageRetrievalStatus.Failed, result.Status);
        Assert.IsNull(result.InstallationPackage);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    // -------------------- ReportInstallationResultAsync --------------------
    [TestMethod]
    public async Task ReportInstallationResultAsync_InstallationSucceeded_SendsRequest()
    {
        // Arrange
        var requestUri = $"deployments/{_agentId}/result";
        SetupReportInstallationResultHttpResponse(requestUri);

        // Act
        await _client.ReportInstallationResultAsync(
            succeeded: true,
            installedVersion: "1.0.1",
            errorMessage: null,
            CancellationToken.None);
        
        // Assert
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    // -------------------- Helper Methods --------------------
    private void SetupGetInstallationPackageHttpResponse(
        string requestUri,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        string? releaseVersion = null,
        string? fileNameStar = null,
        string? fileName = null)
    {
        var content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("package-content")));

        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = fileNameStar,
            FileName = fileName
        };

        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = statusCode == HttpStatusCode.OK ? content : null
        };

        if (!string.IsNullOrWhiteSpace(releaseVersion))
            response.Headers.Add("X-Release-Version", releaseVersion);

        _mockHttpHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>()
        ).ReturnsAsync(response);
    }

    private void SetupReportInstallationResultHttpResponse(
        string requestUri,
        HttpStatusCode statusCode = HttpStatusCode.OK,
        List<string>? errorMessages = null)
    {
        var response = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = errorMessages is not null
                      ? JsonContent.Create(errorMessages)
                      : null
        };

        _mockHttpHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(requestUri)),
            ItExpr.IsAny<CancellationToken>()
        ).ReturnsAsync(response);
    }
}