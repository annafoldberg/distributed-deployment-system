using Moq;
using Moq.Protected;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DeploymentManager.Agent.Infrastructure.Api;
using DeploymentManager.Agent.Infrastructure.Configuration;
using System.Net;
using System.Text;
using System.Net.Http.Headers;

namespace DeploymentManager.Agent.Tests.Infrastructure.Api;

[TestClass]
public sealed class DeploymentManagerApiClientTests
{
    private Mock<HttpMessageHandler> _mockHttpHandler = null!;
    private HttpClient _httpClient = null!;
    private DeploymentManagerApiClient _client = null!;
    private Guid _agentId;
    private string _requestUri = string.Empty;

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
        _requestUri = $"deployments/{_agentId}/package";
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_IsValidResponse_ReturnsInstallationPackage()
    {
        // Arrange
        SetupHttpResponse(fileNameStar: "app-osx-arm64.zip");

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Content);
        Assert.AreEqual("app-osx-arm64.zip", result.FileName);
        var content = await new StreamReader(result.Content).ReadToEndAsync();
        Assert.AreEqual("package-content", content);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(_requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_FileNameStarIsPresent_UsesFileNameStar()
    {
        // Arrange
        SetupHttpResponse(fileNameStar: "app-osx-arm64.zip", fileName: "app-win-x64.zip");

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Content);
        Assert.AreEqual("app-osx-arm64.zip", result.FileName);
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_FileNameStarIsNotPresent_UsesFileName()
    {
        // Arrange
        SetupHttpResponse(fileName: "app-osx-arm64.zip");

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Content);
        Assert.AreEqual("app-osx-arm64.zip", result.FileName);
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_IsNotSuccessStatusCode_ReturnsNull()
    {
        // Arrange
        SetupHttpResponse(HttpStatusCode.NotFound);

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNull(result);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(_requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    [TestMethod]
    public async Task GetInstallationPackageAsync_FileNameMissing_ReturnsNull()
    {
        // Arrange
        SetupHttpResponse();

        // Act
        var result = await _client.GetInstallationPackageAsync(CancellationToken.None);

        // Assert
        Assert.IsNull(result);
        _mockHttpHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(_requestUri)),
            ItExpr.IsAny<CancellationToken>());
    }

    // -------------------- Helper Methods --------------------
    private void SetupHttpResponse(HttpStatusCode statusCode = HttpStatusCode.OK, string? fileNameStar = null, string? fileName = null)
    {
        var content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("package-content")));

        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = fileNameStar,
            FileName = fileName
        };

        _mockHttpHandler.Protected().Setup<Task<HttpResponseMessage>>(
            "SendAsync",
            ItExpr.Is<HttpRequestMessage>(r =>
                r.RequestUri!.AbsoluteUri.Contains(_requestUri)),
            ItExpr.IsAny<CancellationToken>()
        ).ReturnsAsync(new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = statusCode == HttpStatusCode.OK ? content : null
        });
    }
}

// Sources:
// ContentDispositionHeaderValue: https://learn.microsoft.com/en-us/dotnet/api/system.net.http.headers.contentdispositionheadervalue?view=net-10.0