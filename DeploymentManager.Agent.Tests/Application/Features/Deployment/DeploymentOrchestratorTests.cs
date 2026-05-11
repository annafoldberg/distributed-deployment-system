using Moq;
using Microsoft.Extensions.Logging;
using DeploymentManager.Agent.Application.Features.Deployment.Interfaces;
using DeploymentManager.Agent.Application.Features.Deployment;
using DeploymentManager.Agent.Application.Features.Deployment.Models;
using DeploymentManager.Agent.Application.Features.Deployment.Results;

namespace DeploymentManager.Agent.Tests.Application.Features.Deployment;

[TestClass]
public sealed class DeploymentOrchestratorTests
{
    private Mock<IDeploymentManagerApiClient> _mockApiClient = null!;
    private Mock<IPackageInstaller> _mockPackageInstaller = null!;
    private DeploymentOrchestrator _orchestrator = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockApiClient = new Mock<IDeploymentManagerApiClient>();
        _mockPackageInstaller = new Mock<IPackageInstaller>();
        var logger = Mock.Of<ILogger<DeploymentOrchestrator>>();
        _orchestrator = new DeploymentOrchestrator(_mockApiClient.Object, _mockPackageInstaller.Object, logger);
    }

    [TestMethod]
    public async Task ExecuteAsync_PackageIsRetrievedAndInstalled_ReturnsSucceeded()
    {
        // Arrange
        var installationPackage = new InstallationPackage(
            new MemoryStream(),
            "app-osx-arm64.zip");
        
        _mockApiClient.Setup(c =>
            c.GetInstallationPackageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(installationPackage);

        _mockPackageInstaller.Setup(i =>
            i.InstallPackageAsync(installationPackage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InstallationResult.Succeeded);

        // Act
        var result = await _orchestrator.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(DeploymentResult.Succeeded, result);
        _mockApiClient.Verify(c =>
            c.GetInstallationPackageAsync(It.IsAny<CancellationToken>()),
            Times.Once());
        _mockPackageInstaller.Verify(i =>
            i.InstallPackageAsync(installationPackage, It.IsAny<CancellationToken>()),
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
        _mockPackageInstaller.Verify(i =>
            i.InstallPackageAsync(It.IsAny<InstallationPackage>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [TestMethod]
    public async Task ExecuteAsync_PackageIsNotInstalled_ReturnsInstallationFailed()
    {
        // Arrange
        var installationPackage = new InstallationPackage(
            new MemoryStream(),
            "app-osx-arm64.zip");
        
        _mockApiClient.Setup(c =>
            c.GetInstallationPackageAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(installationPackage);

        _mockPackageInstaller.Setup(i =>
            i.InstallPackageAsync(installationPackage, It.IsAny<CancellationToken>()))
            .ReturnsAsync(InstallationResult.Failed);

        // Act
        var result = await _orchestrator.ExecuteAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(DeploymentResult.InstallationFailed, result);
        _mockApiClient.Verify(c =>
            c.GetInstallationPackageAsync(It.IsAny<CancellationToken>()),
            Times.Once());
        _mockPackageInstaller.Verify(i =>
            i.InstallPackageAsync(installationPackage, It.IsAny<CancellationToken>()),
            Times.Once());
    }
}