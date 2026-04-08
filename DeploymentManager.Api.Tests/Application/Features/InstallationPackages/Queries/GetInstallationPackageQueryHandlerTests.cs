using DeploymentManager.Api.Application.Features.InstallationPackages.Interfaces;
using DeploymentManager.Api.Application.Features.InstallationPackages.Models;
using DeploymentManager.Api.Application.Features.InstallationPackages.Queries;
using Moq;

namespace DeploymentManager.Api.Tests.Application.Features.InstallationPackages.Queries;

[TestClass]
public sealed class GetInstallationPackageQueryHandlerTests
{
    private Mock<IPackageProvider> _mockProvider = null!;
    private GetInstallationPackageQueryHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockProvider = new Mock<IPackageProvider>();
        _handler = new GetInstallationPackageQueryHandler(_mockProvider.Object);
    }

    [TestMethod]
    public async Task Handle_PackageFound_ReturnsOkResult()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);

        var package = new InstallationPackage(
            new MemoryStream(),
            "application/zip",
            "app-osx-arm64.zip");

        _mockProvider.Setup(p =>
            p.FetchPackageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(package, result.Value);
        _mockProvider.Verify(p =>
            p.FetchPackageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Once());
    }

    [TestMethod]
    public async Task Handle_PackageNotFound_ReturnsFailedResult()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);

        _mockProvider.Setup(p =>
            p.FetchPackageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((InstallationPackage?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        _mockProvider.Verify(p =>
            p.FetchPackageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
                Times.Once());
    }
}