using Moq;
using Microsoft.Extensions.Logging;
using DeploymentManager.Agent.Application.Features.Deployment.Interfaces;
using DeploymentManager.Agent.Application.Features.Deployment;
using DeploymentManager.Agent.Application.Features.Deployment.Models;

namespace DeploymentManager.Agent.Tests.Application.Features.Deployment;

[TestClass]
public sealed class DeploymentOrchestratorTests
{
    private Mock<IDeploymentManagerApiClient> _mockApiClient = null!;
    private DeploymentOrchestrator _orchestrator = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockApiClient = new Mock<IDeploymentManagerApiClient>();
        var logger = Mock.Of<ILogger<DeploymentOrchestrator>>();
        _orchestrator = new DeploymentOrchestrator(_mockApiClient.Object, logger);
    }

    [TestMethod]
    public async Task ExecuteAsync_PackageIsRetrieved_ReturnsSucceeded()
    {
        // Arrange
        var installationPackage = new InstallationPackage(
            new MemoryStream(),
            "app-osx-arm64.zip");
        
        _mockApiClient.Setup(c =>
            c.GetInstallationPackageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(installationPackage);

        // Act
        var result = await _orchestrator.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(DeploymentResult.Succeeded, result);
        _mockApiClient.Verify(c =>
            c.GetInstallationPackageAsync(It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task ExecuteAsync_PackageIsNotRetrieved_ReturnsRetrievalFailed()
    {
        // Arrange
        _mockApiClient.Setup(c =>
            c.GetInstallationPackageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((InstallationPackage?)null);

        // Act
        var result = await _orchestrator.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(DeploymentResult.RetrievalFailed, result);
        _mockApiClient.Verify(c =>
            c.GetInstallationPackageAsync(It.IsAny<CancellationToken>()),
            Times.Once());
    }
}