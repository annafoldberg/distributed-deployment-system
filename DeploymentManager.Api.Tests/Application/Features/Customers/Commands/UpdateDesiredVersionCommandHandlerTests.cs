using Moq;
using MockQueryable.Moq;
using Microsoft.Extensions.Logging;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Customers.Commands;
using DeploymentManager.Api.Domain.Entities;
using DeploymentManager.Api.Application.Features.Customers.Errors;

namespace DeploymentManager.Api.Tests.Application.Features.Customers.Commands;

[TestClass]
public sealed class UpdateDesiredVersionCommandHandlerTests
{
    private Mock<IDeploymentManagerDbContext> _mockContext = null!;
    private UpdateDesiredVersionCommandHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockContext = new Mock<IDeploymentManagerDbContext>();
        var logger = Mock.Of<ILogger<UpdateDesiredVersionCommandHandler>>();
        _handler = new UpdateDesiredVersionCommandHandler(_mockContext.Object, logger);
    }

    [TestMethod]
    public async Task Handle_DesiredVersionExists_ReturnsSuccess()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var desiredVersion = "1.0.0";
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);
        
        var (customer, release) = CreateDatabaseEntities(customerId);
        SetupDatabaseContext(customer, release);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Handle_CustomerDoesNotExist_ReturnsCustomerNotFoundError()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var desiredVersion = "1.0.0";
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);

        var (_, release) = CreateDatabaseEntities(customerId);
        SetupDatabaseContext(release: release);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(CustomerNotFoundError));
    }

    [TestMethod]
    public async Task Handle_DesiredVersionDoesNotExist_ReturnsReleaseNotFoundError()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var desiredVersion = "1.0.0";
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);

        var (customer, _) = CreateDatabaseEntities(customerId);
        SetupDatabaseContext(customer);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(ReleaseNotFoundError));
    }

    [TestMethod]
    public async Task Handle_DesiredVersionAlreadySet_ReturnsDesiredVersionAlreadySetError()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var desiredVersion = "1.0.0";
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);

        var (customer, release) = CreateDatabaseEntities(customerId);
        customer.DesiredReleaseId = release.Id;
        SetupDatabaseContext(customer, release);
        
        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(DesiredVersionAlreadySetError));
    }

    // -------------------- Helper Methods --------------------
    private (Customer, Release) CreateDatabaseEntities(Guid customerId)
    {
        var release = new Release
        {
            Id = 1,
            Version = "1.0.0"
        };

        var customer = new Customer
        {
            Id = 1,
            PublicId = customerId,
            CompanyName = "Demo Company"
        };

        return (customer, release);
    }

    private void SetupDatabaseContext(Customer? customer = null, Release? release = null)
    {
        var customers = customer is null
                        ? new List<Customer>().BuildMockDbSet()
                        : new List<Customer>{ customer }.BuildMockDbSet();
        
        var releases = release is null
                       ? new List<Release>().BuildMockDbSet()
                       : new List<Release>{ release }.BuildMockDbSet();

        _mockContext.Setup(c => c.Customers).Returns(customers.Object);
        _mockContext.Setup(r => r.Releases).Returns(releases.Object);
    }
}