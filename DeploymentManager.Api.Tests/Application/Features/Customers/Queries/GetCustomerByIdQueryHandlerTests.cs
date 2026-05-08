using Moq;
using MockQueryable.Moq;
using Microsoft.Extensions.Logging;
using DeploymentManager.Api.Application.Common.Interfaces;
using DeploymentManager.Api.Application.Features.Customers.Queries;
using DeploymentManager.Api.Domain.Entities;
using DeploymentManager.Api.Application.Features.Customers.Errors;

namespace DeploymentManager.Api.Tests.Application.Features.Customers.Queries;

[TestClass]
public sealed class GetCustomerByIdQueryHandlerTests
{
    private Mock<IDeploymentManagerDbContext> _mockContext = null!;
    private GetCustomerByIdQueryHandler _handler = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockContext = new Mock<IDeploymentManagerDbContext>();
        var logger = Mock.Of<ILogger<GetCustomerByIdQueryHandler>>();
        _handler = new GetCustomerByIdQueryHandler(_mockContext.Object, logger);
    }

    [TestMethod]
    public async Task Handle_CustomerExists_ReturnsCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(customerId);
        
        var customer = CreateDatabaseEntities(customerId);
        SetupDatabaseContext(customer);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(customer.PublicId, result.Value.Id);
        Assert.AreEqual(customer.CompanyName, result.Value.CompanyName);
        Assert.AreEqual(customer.DesiredRelease!.Version, result.Value.DesiredVersion);
        Assert.HasCount(2, result.Value.Agents);
        Assert.AreEqual(customer.Agents[0].PublicId, result.Value.Agents[0].Id);
    }

    [TestMethod]
    public async Task Handle_CustomerDoesNotExist_ReturnsCustomerNotFoundError()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(customerId);

        SetupDatabaseContext();
        
        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, result.Errors);
        Assert.IsInstanceOfType(result.Errors[0], typeof(CustomerNotFoundError));
    }

    // -------------------- Helper Methods --------------------
    private Customer CreateDatabaseEntities(Guid customerId)
    {
        var currentRelease = new Release
        {
            Id = 1,
            Version = "1.0.0"
        };

        var desiredRelease = new Release
        {
            Id = 2,
            Version = "2.0.0"
        };

        var customer = new Customer
        {
            Id = 1,
            PublicId = customerId,
            CompanyName = "Test Company",
            DesiredRelease = desiredRelease
        };

        var agentOne = new Agent
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            Platform = "osx-arm64",
            Customer = customer
        };

        var agentTwo = new Agent
        {
            Id = 2,
            PublicId = Guid.NewGuid(),
            Platform = "win-x64",
            Customer = customer
        };

        var installationOne = new Installation
        {
            Id = 1,
            AgentId = agentOne.Id,
            Agent = agentOne,
            Release = currentRelease
        };

        var installationTwo = new Installation
        {
            Id = 2,
            AgentId = agentTwo.Id,
            Agent = agentTwo,
            Release = currentRelease
        };

        agentOne.Installation = installationOne;
        agentTwo.Installation = installationTwo;

        customer.Agents.Add(agentOne);
        customer.Agents.Add(agentTwo);

        return customer;
    }

    private void SetupDatabaseContext(Customer? customer = null)
    {
        var customers = customer is null
                        ? new List<Customer>().BuildMockDbSet()
                        : new List<Customer>{ customer }.BuildMockDbSet();

        _mockContext.Setup(c => c.Customers).Returns(customers.Object);
    }
}