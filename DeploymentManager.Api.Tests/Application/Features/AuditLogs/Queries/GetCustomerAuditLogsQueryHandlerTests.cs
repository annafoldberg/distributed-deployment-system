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
public sealed class GetCustomerAuditLogsQueryHandlerTests
{
    private Mock<IDeploymentManagerDbContext> _mockContext = null!;
    private GetCustomerAuditLogsQueryHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockContext = new Mock<IDeploymentManagerDbContext>();
        var logger = Mock.Of<ILogger<GetCustomerAuditLogsQueryHandler>>();
        _handler = new GetCustomerAuditLogsQueryHandler(_mockContext.Object, logger);
    }

    [TestMethod]
    public async Task Handle_CustomerExists_ReturnsSuccessWithAuditLogs()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerAuditLogsQuery(customerId);
        
        (var customer, var auditLogs) = CreateDatabaseEntities(customerId);
        SetupDatabaseContext(customer, auditLogs);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, result.Value);
        Assert.IsTrue(result.Value.All(l => l.CustomerId == customer.Id));
        Assert.IsTrue(result.Value.All(l => l.AgentId is null));
        Assert.AreEqual(2, result.Value[0].Id);
        Assert.AreEqual(1, result.Value[1].Id);
        Assert.AreEqual(AuditLogLevel.Information, result.Value[0].Level);
        Assert.AreEqual(AuditLogLevel.Warning, result.Value[1].Level);
    }

    [TestMethod]
    public async Task Handle_CustomerDoesNotExist_ReturnsCustomerNotFoundError()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerAuditLogsQuery(customerId);

        SetupDatabaseContext();
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(CustomerNotFoundError));
    }

    [TestMethod]
    public async Task Handle_CustomerHasNoAuditLogs_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerAuditLogsQuery(customerId);

        (var customer, _) = CreateDatabaseEntities(customerId);
        SetupDatabaseContext(customer);
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(0, result.Value);
    }

    // -------------------- Helper Methods --------------------
    private (Customer, List<AuditLog>) CreateDatabaseEntities(Guid customerId)
    {
        var desiredRelease = new Release
        {
            Id = 1,
            Version = "1.0.0"
        };

        var customerOne = new Customer
        {
            Id = 1,
            PublicId = customerId,
            CompanyName = "Test Company One",
            DesiredRelease = desiredRelease
        };

        var customerTwo = new Customer
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            CompanyName = "Test Company Two",
            DesiredRelease = desiredRelease
        };

        var agent = new Agent
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Platform = "osx-arm64",
            Customer = customerOne
        };

        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = 1,
                CreatedAt = new DateTimeOffset(2026, 6, 1, 1, 0, 0, TimeSpan.Zero),
                Level = AuditLogLevel.Warning,
                Message = "Desired version update failed.",
                CustomerId = customerOne.Id
            },
            new()
            {
                Id = 2,
                CreatedAt = new DateTimeOffset(2026, 6, 1, 2, 0, 0, TimeSpan.Zero),
                Level = AuditLogLevel.Information,
                Message = "Desired version update skipped.",
                CustomerId = customerOne.Id
            },
            new()
            {
                Id = 3,
                CreatedAt = new DateTimeOffset(2026, 6, 1, 3, 0, 0, TimeSpan.Zero),
                Level = AuditLogLevel.Information,
                Message = "Desired version updated.",
                CustomerId = customerTwo.Id
            },
            new()
            {
                Id = 4,
                CreatedAt = new DateTimeOffset(2026, 6, 1, 4, 0, 0, TimeSpan.Zero),
                Level = AuditLogLevel.Information,
                Message = "Successfully installed version.",
                AgentId = agent.Id
            }
        };

        return (customerOne, auditLogs);
    }

    private void SetupDatabaseContext(Customer? customer = null, List<AuditLog>? auditLogs = null)
    {
        var customersDbSet = customer is null
                             ? new List<Customer>().BuildMockDbSet()
                             : new List<Customer>{ customer }.BuildMockDbSet();

        var auditLogsDbSet = auditLogs is null
                             ? new List<AuditLog>().BuildMockDbSet()
                             : auditLogs.BuildMockDbSet();

        _mockContext.Setup(c => c.Customers).Returns(customersDbSet.Object);
        _mockContext.Setup(c => c.AuditLogs).Returns(auditLogsDbSet.Object);
    }
}