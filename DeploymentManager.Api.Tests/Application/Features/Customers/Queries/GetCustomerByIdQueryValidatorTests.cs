using DeploymentManager.Api.Application.Features.Customers.Queries;

namespace DeploymentManager.Api.Tests.Application.Features.Customers.Queries;

[TestClass]
public sealed class GetCustomerByIdQueryValidatorTests
{
    [TestMethod]
    public async Task Validate_ValidCustomerId_Succeeds()
    {
        // Arrange
        var validator = new GetCustomerByIdQueryValidator();
        var customerId = Guid.NewGuid();
        var query = new GetCustomerByIdQuery(customerId);

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
        var validator = new GetCustomerByIdQueryValidator();
        var customerId = Guid.Empty;
        var query = new GetCustomerByIdQuery(customerId);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(GetCustomerByIdQuery.CustomerId), result.Errors[0].PropertyName);
    }
}