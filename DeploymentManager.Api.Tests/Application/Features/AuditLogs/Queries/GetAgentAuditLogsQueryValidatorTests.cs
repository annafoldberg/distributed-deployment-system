using DeploymentManager.Api.Application.Features.AuditLogs.Queries;

namespace DeploymentManager.Api.Tests.Application.Features.AuditLogs.Queries;

[TestClass]
public sealed class GetAgentAuditLogsQueryValidatorTests
{
    [TestMethod]
    public async Task Validate_ValidCAgentId_Succeeds()
    {
        // Arrange
        var validator = new GetAgentAuditLogsQueryValidator();
        var agentId = Guid.NewGuid();
        var query = new GetAgentAuditLogsQuery(agentId);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.HasCount(0, result.Errors);
    }

    [TestMethod]
    public async Task Validate_EmptyCustomerId_Fails()
    {
        // Arrange
        var validator = new GetAgentAuditLogsQueryValidator();
        var agentId = Guid.Empty;
        var query = new GetAgentAuditLogsQuery(agentId);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(GetAgentAuditLogsQuery.AgentId), result.Errors[0].PropertyName);
    }
}