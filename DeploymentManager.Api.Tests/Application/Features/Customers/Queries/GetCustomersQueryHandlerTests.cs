using Moq;
using MockQueryable.Moq;
using Microsoft.Extensions.Logging;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Customers.Queries;
using DeploymentManager.Api.Domain.Entities;

namespace DeploymentManager.Api.Tests.Application.Features.Customers.Queries;

[TestClass]
public sealed class GetCustomersQueryHandlerTests
{
    private Mock<IDeploymentManagerDbContext> _mockContext = null!;
    private GetCustomersQueryHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockContext = new Mock<IDeploymentManagerDbContext>();
        var logger = Mock.Of<ILogger<GetCustomersQueryHandler>>();
        _handler = new GetCustomersQueryHandler(_mockContext.Object, logger);
    }

    [TestMethod]
    public async Task Handle_CustomersExist_ReturnsSuccessWithCustomers()
    {
        // Arrange
        var query = new GetCustomersQuery();
        
        var customers = CreateDatabaseEntities();
        SetupDatabaseContext(customers);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, result.Value);
        Assert.AreEqual(customers[0].PublicId, result.Value[0].Id);
        Assert.AreEqual(customers[0].CompanyName, result.Value[0].CompanyName);
        Assert.AreEqual(customers[0].DesiredRelease!.Version, result.Value[0].DesiredVersion);
    }

    [TestMethod]
    public async Task Handle_NoCustomersExist_ReturnsSuccessWithNoCustomers()
    {
        // Arrange
        var query = new GetCustomersQuery();
        
        SetupDatabaseContext();
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(0, result.Value);
    }

    // -------------------- Helper Methods --------------------
    private List<Customer> CreateDatabaseEntities()
    {
        var desiredRelease = new Release
        {
            Id = 1,
            Version = "2.0.0"
        };

        var customerOne = new Customer
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
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

        return new List<Customer>{ customerOne, customerTwo };
    }

    private void SetupDatabaseContext(List<Customer>? customers = null)
    {
        var customersDbSet = customers is null
                             ? new List<Customer>().BuildMockDbSet()
                             : customers.BuildMockDbSet();

        _mockContext.Setup(c => c.Customers).Returns(customersDbSet.Object);
    }
}