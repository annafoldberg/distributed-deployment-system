using Moq;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.AuditLogs.Queries;
using Microsoft.Extensions.Logging;
using DeploymentManager.Api.Application.Features.Common.Errors;
using DeploymentManager.Api.Domain.Entities;
using MockQueryable.Moq;
using DeploymentManager.Api.Domain.Enums;

namespace DeploymentManager.Api.Tests.Application.Features.AuditLogs.Queries;

[TestClass]
public sealed class GetAgentAuditLogsQueryHandlerTests
{
    private Mock<IDeploymentManagerDbContext> _mockContext = null!;
    private GetAgentAuditLogsQueryHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockContext = new Mock<IDeploymentManagerDbContext>();
        var logger = Mock.Of<ILogger<GetAgentAuditLogsQueryHandler>>();
        _handler = new GetAgentAuditLogsQueryHandler(_mockContext.Object, logger);
    }

    [TestMethod]
    public async Task Handle_AgentExists_ReturnsSuccessWithAuditLogs()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetAgentAuditLogsQuery(agentId);
        
        (var agent, var auditLogs) = CreateDatabaseEntities(agentId);
        SetupDatabaseContext(agent, auditLogs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, result.Value);
        Assert.IsTrue(result.Value.All(l => l.AgentId == agent.Id));
        Assert.IsTrue(result.Value.All(l => l.CustomerId is null));
        Assert.AreEqual(2, result.Value[0].Id);
        Assert.AreEqual(1, result.Value[1].Id);
        Assert.AreEqual(AuditLogLevel.Information, result.Value[0].Level);
        Assert.AreEqual(AuditLogLevel.Warning, result.Value[1].Level);
    }

    [TestMethod]
    public async Task Handle_AgentDoesNotExist_ReturnsAgentNotFoundError()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetAgentAuditLogsQuery(agentId);

        SetupDatabaseContext();
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(AgentNotFoundError));
    }

    [TestMethod]
    public async Task Handle_AgentHasNoAuditLogs_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var agentId = Guid.NewGuid();
        var query = new GetAgentAuditLogsQuery(agentId);

        (var agent, _) = CreateDatabaseEntities(agentId);
        SetupDatabaseContext(agent);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(0, result.Value);
    }

    // -------------------- Helper Methods --------------------
    private (Agent, List<AuditLog>) CreateDatabaseEntities(Guid agentId)
    {
        var desiredRelease = new Release
        {
            Id = 1,
            Version = "1.0.0"
        };

        var customer = new Customer
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            CompanyName = "Test Company",
            DesiredRelease = desiredRelease
        };

        var agentOne = new Agent
        {
            Id = 1,
            PublicId = agentId,
            Platform = "osx-arm64",
            Customer = customer
        };

        var agentTwo = new Agent
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            Platform = "osx-arm64",
            Customer = customer
        };

        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                CreatedAt = new DateTimeOffset(2026, 6, 1, 1, 0, 0, TimeSpan.Zero),
                Level = AuditLogLevel.Warning,
                Message = "Installation report failed.",
                AgentId = agentOne.Id
            },
            new()
            {
                Id = 2,
                CreatedAt = new DateTimeOffset(2026, 6, 1, 2, 0, 0, TimeSpan.Zero),
                Level = AuditLogLevel.Information,
                Message = "Installation report skipped.",
                AgentId = agentOne.Id
            },
            new()
            {
                Id = 3,
                CreatedAt = new DateTimeOffset(2026, 6, 1, 3, 0, 0, TimeSpan.Zero),
                Level = AuditLogLevel.Information,
                Message = "Successfully installed version.",
                AgentId = agentTwo.Id
            },
            new()
            {
                Id = 4,
                CreatedAt = new DateTimeOffset(2026, 6, 1, 4, 0, 0, TimeSpan.Zero),
                Level = AuditLogLevel.Information,
                Message = "Desired version updated.",
                CustomerId = customer.Id
            }
        };

        return (agentOne, auditLogs);
    }

    private void SetupDatabaseContext(Agent? agent = null, List<AuditLog>? auditLogs = null)
    {
        var agentsDbSet = agent is null
                          ? new List<Agent>().BuildMockDbSet()
                          : new List<Agent>{ agent }.BuildMockDbSet();

        var auditLogsDbSet = auditLogs is null
                             ? new List<AuditLog>().BuildMockDbSet()
                             : auditLogs.BuildMockDbSet();

        _mockContext.Setup(c => c.Agents).Returns(agentsDbSet.Object);
        _mockContext.Setup(c => c.AuditLogs).Returns(auditLogsDbSet.Object);
    }
}