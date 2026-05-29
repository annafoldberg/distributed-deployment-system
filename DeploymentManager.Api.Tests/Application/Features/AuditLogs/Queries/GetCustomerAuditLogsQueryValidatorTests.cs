using DeploymentManager.Api.Application.Features.AuditLogs.Queries;

namespace DeploymentManager.Api.Tests.Application.Features.AuditLogs.Queries;

[TestClass]
public sealed class GetCustomerAuditLogsQueryValidatorTests
{
    [TestMethod]
    public async Task Validate_ValidCustomerId_Succeeds()
    {
        // Arrange
        var validator = new GetCustomerAuditLogsQueryValidator();
        var customerId = Guid.NewGuid();
        var query = new GetCustomerAuditLogsQuery(customerId);

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
        var validator = new GetCustomerAuditLogsQueryValidator();
        var customerId = Guid.Empty;
        var query = new GetCustomerAuditLogsQuery(customerId);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(GetCustomerAuditLogsQuery.CustomerId), result.Errors[0].PropertyName);
    }
}