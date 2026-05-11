using Moq;
using MockQueryable.Moq;
using Microsoft.Extensions.Logging;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Domain.Entities;
using DeploymentManager.Api.Application.Features.Common.Errors;
using DeploymentManager.Api.Application.Features.Deployments.Commands;
using DeploymentManager.Api.Application.Features.Deployments.Errors;

namespace DeploymentManager.Api.Tests.Application.Features.Deployments.Commands;

[TestClass]
public sealed class ReportInstallationResultCommandHandlerTests
{
    private Mock<IDeploymentManagerDbContext> _mockContext = null!;
    private ReportInstallationResultCommandHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockContext = new Mock<IDeploymentManagerDbContext>();
        var logger = Mock.Of<ILogger<ReportInstallationResultCommandHandler>>();
        _handler = new ReportInstallationResultCommandHandler(_mockContext.Object, logger);
    }

    [TestMethod]
    public async Task Handle_AgentExistsAndInstallationDoesNotExist_CreatesInstallation()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var installedVersion = "1.0.1";
        var succeeded = true;
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, null);
        
        var (agent, _, installedRelease) = CreateDatabaseEntities(agentId, null, installedVersion);
        SetupDatabaseContext(agent, null, installedRelease);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        _mockContext.Verify(c => c.Installations.Add(
            It.Is<Installation>(i =>
                i.AgentId == agent.Id &&
                i.ReleaseId == installedRelease!.Id)),
            Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AgentAndInstallationExist_UpdatesExistingInstallation()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var currentVersion = "1.0.0";
        var installedVersion = "1.0.1";
        var succeeded = true;
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, null);
        
        var (agent, installation, installedRelease) = CreateDatabaseEntities(agentId, currentVersion, installedVersion);
        SetupDatabaseContext(agent, installation, installedRelease);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(installedRelease!.Id, installation!.ReleaseId);
        _mockContext.Verify(c => c.Installations.Add(It.IsAny<Installation>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_AgentExistsAndInstallationIsAlreadyInstalled_ReturnsSuccess()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var version = "1.0.0";
        var succeeded = true;
        var command = new ReportInstallationResultCommand(agentId, succeeded, version, null);
        
        var (agent, installation, installedRelease) = CreateDatabaseEntities(agentId, version, version);
        SetupDatabaseContext(agent, installation, installedRelease);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        _mockContext.Verify(c => c.Installations.Add(It.IsAny<Installation>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Handle_InstallationFailed_ReturnsSuccess()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var currentVersion = "1.0.0";
        var installedVersion = "1.0.1";
        var succeeded = false;
        var errorMessage = "Error";
        var command = new ReportInstallationResultCommand(agentId, succeeded, null, errorMessage);
        
        var (agent, installation, installedRelease) = CreateDatabaseEntities(agentId, currentVersion, installedVersion);
        SetupDatabaseContext(agent, installation, installedRelease);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_AgentDoesNotExist_ReturnsAgentNotFoundError()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var currentVersion = "1.0.0";
        var installedVersion = "1.0.1";
        var succeeded = true;
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, null);
        
        var (_, installation, installedRelease) = CreateDatabaseEntities(agentId, currentVersion, installedVersion);
        SetupDatabaseContext(installation: installation, release: installedRelease);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(AgentNotFoundError));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Handle_InstalledReleaseDoesNotExist_ReturnsReleaseNotFoundError()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var currentVersion = "1.0.0";
        var installedVersion = "1.0.1";
        var succeeded = true;
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, null);
        
        var (agent, installation, _) = CreateDatabaseEntities(agentId, currentVersion, installedVersion);
        SetupDatabaseContext(agent, installation);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(ReleaseNotFoundError));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // -------------------- Helper Methods --------------------
    private (Agent, Installation?, Release?) CreateDatabaseEntities(Guid agentId, string? currentVersion, string? installedVersion)
    {

        var currentRelease = currentVersion is not null
                             ? new Release { Id = 1, Version = currentVersion }
                             : null;

        var agent = new Agent
        {
            Id = 1,
            PublicId = agentId,
        };

        var installedRelease = installedVersion is not null
                               ? new Release { Id = 2, Version = installedVersion }
                               : null;

        var installation = currentRelease is not null
                           ? new Installation
                             {
                                Id = 1,
                                AgentId = agent.Id,
                                Agent = agent,
                                ReleaseId = currentRelease.Id,
                                Release = currentRelease
                             }
                            : null;

        return (agent, installation, installedRelease);
    }

    private void SetupDatabaseContext(Agent? agent = null, Installation? installation = null, Release? release = null)
    {
        var agents = agent is null
                     ? new List<Agent>().BuildMockDbSet()
                     : new List<Agent>{ agent }.BuildMockDbSet();
        
        var releases = release is null
                       ? new List<Release>().BuildMockDbSet()
                       : new List<Release>{ release }.BuildMockDbSet();

        var installations = installation is null
                            ? new List<Installation>().BuildMockDbSet()
                            : new List<Installation>{ installation }.BuildMockDbSet();

        _mockContext.Setup(c => c.Agents).Returns(agents.Object);
        _mockContext.Setup(c => c.Releases).Returns(releases.Object);
        _mockContext.Setup(c => c.Installations).Returns(installations.Object);
    }
}