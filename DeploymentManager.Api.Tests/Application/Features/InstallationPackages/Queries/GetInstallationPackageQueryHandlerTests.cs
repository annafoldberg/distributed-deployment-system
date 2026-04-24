using Moq;
using MockQueryable.Moq;
using DeploymentManager.Api.Application.Features.InstallationPackages.Interfaces;
using DeploymentManager.Api.Application.Features.InstallationPackages.Dtos;
using DeploymentManager.Api.Application.Features.InstallationPackages.Queries;
using DeploymentManager.Api.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using DeploymentManager.Api.Domain.Entities;
using DeploymentManager.Api.Application.Features.InstallationPackages.Errors;

namespace DeploymentManager.Api.Tests.Application.Features.InstallationPackages.Queries;

[TestClass]
public sealed class GetInstallationPackageQueryHandlerTests
{
    private Mock<IPackageProvider> _mockProvider = null!;
    private Mock<IDeploymentManagerDbContext> _mockContext = null!;
    private GetInstallationPackageQueryHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockProvider = new Mock<IPackageProvider>();
        _mockContext = new Mock<IDeploymentManagerDbContext>();
        var logger = Mock.Of<ILogger<GetInstallationPackageQueryHandler>>();
        _handler = new GetInstallationPackageQueryHandler(_mockProvider.Object, _mockContext.Object, logger);
    }

    [TestMethod]
    public async Task Handle_PackageExists_ReturnsOkResultWithPackage()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);
        
        (var agent, var installation) = CreateDatabaseEntities(agentId, "1.0.0", "2.0.0");
        SetupDatabaseContext(agent, installation);

        var package = new InstallationPackageDto(
            new MemoryStream(),
            "application/zip",
            "app-osx-arm64.zip");

        _mockProvider.Setup(p =>
            p.FetchPackageAsync("osx-arm64", "2.0.0", It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(package, result.Value);
        _mockProvider.Verify(p =>
            p.FetchPackageAsync("osx-arm64", "2.0.0", It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task Handle_NoInstallationExists_ReturnsOkResultWithPackage()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);
        
        (var agent, _) = CreateDatabaseEntities(agentId, "1.0.0", "2.0.0");
        SetupDatabaseContext(agent);

        var package = new InstallationPackageDto(
            new MemoryStream(),
            "application/zip",
            "app-osx-arm64.zip");

        _mockProvider.Setup(p =>
            p.FetchPackageAsync("osx-arm64", "2.0.0", It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(package, result.Value);
        _mockProvider.Verify(p =>
            p.FetchPackageAsync("osx-arm64", "2.0.0", It.IsAny<CancellationToken>()),
            Times.Once());
    }

    [TestMethod]
    public async Task Handle_AgentDoesNotExist_ReturnsAgentNotFoundError()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);

        SetupDatabaseContext();
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(AgentNotFoundError));
        _mockProvider.Verify(p =>
            p.FetchPackageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [TestMethod]
    public async Task Handle_DesiredReleaseNotSet_ReturnsDesiredReleaseNotSetError()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);
        
        (var agent, var installation) = CreateDatabaseEntities(agentId, "1.0.0", null);
        SetupDatabaseContext(agent, installation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(DesiredReleaseNotSetError));
        _mockProvider.Verify(p =>
            p.FetchPackageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [TestMethod]
    public async Task Handle_ReleaseAlreadyInstalled_ReturnsNoUpdateRequiredError()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);
        
        (var agent, var installation) = CreateDatabaseEntities(agentId, "1.0.0", "1.0.0");
        SetupDatabaseContext(agent, installation);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(NoUpdateRequiredError));
        _mockProvider.Verify(p =>
            p.FetchPackageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never());
    }

    [TestMethod]
    public async Task Handle_PackageDoesNotExist_ReturnsPackageNotFoundError()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);
        
        (var agent, var installation) = CreateDatabaseEntities(agentId, "1.0.0", "2.0.0");
        SetupDatabaseContext(agent, installation);

        _mockProvider.Setup(p =>
            p.FetchPackageAsync("osx-arm64", "2.0.0", It.IsAny<CancellationToken>()))
            .ReturnsAsync((InstallationPackageDto?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(PackageNotFoundError));
        _mockProvider.Verify(p =>
            p.FetchPackageAsync("osx-arm64", "2.0.0", It.IsAny<CancellationToken>()),
            Times.Once());
    }

    // -------------------- Helper Methods --------------------
    private (Agent, Installation) CreateDatabaseEntities(Guid agentId, string currentVersion, string? desiredVersion)
    {
        var currentRelease = new Release
        {
            Id = 1,
            Version = currentVersion
        };

        var desiredRelease = desiredVersion is null
                             ? null
                             : desiredVersion == currentVersion
                                ? currentRelease
                                : new Release { Id = 2, Version = desiredVersion };

        var customer = new Customer
        {
            Id = 1,
            CompanyName = "Demo Company",
            DesiredRelease = desiredRelease
        };

        var agent = new Agent
        {
            Id = 1,
            PublicId = agentId,
            Platform = "osx-arm64",
            Customer = customer
        };

        var installation = new Installation
        {
            Id = 1,
            AgentId = agent.Id,
            Agent = agent,
            Release = currentRelease
        };

        return (agent, installation);
    }

    private void SetupDatabaseContext(Agent? agent = null, Installation? installation = null)
    {
        var agents = agent is null
                     ? new List<Agent>().BuildMockDbSet()
                     : new List<Agent>{ agent }.BuildMockDbSet();

        var installations = installation is null
                            ? new List<Installation>().BuildMockDbSet()
                            : new List<Installation>{ installation }.BuildMockDbSet();

        _mockContext.Setup(c => c.Agents).Returns(agents.Object);
        _mockContext.Setup(c => c.Installations).Returns(installations.Object);
    }
}

// Sources:
// MockQueryable: https://www.nuget.org/packages/MockQueryable.Moq/