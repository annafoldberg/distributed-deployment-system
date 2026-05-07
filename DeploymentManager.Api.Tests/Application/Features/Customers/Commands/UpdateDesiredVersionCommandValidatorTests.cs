using DeploymentManager.Api.Application.Features.Customers.Commands;

namespace DeploymentManager.Api.Tests.Application.Features.Customers.Commands;

[TestClass]
public sealed class UpdateDesiredVersionCommandValidatorTests
{
    [TestMethod]
    public async Task Validate_ValidCustomerIdAndVersion_Succeeds()
    {
        // Arrange
        var validator = new UpdateDesiredVersionCommandValidator();
        var customerId = Guid.NewGuid();
        var desiredVersion = "1.0.0";
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.HasCount(0, result.Errors);
    }

    [TestMethod]
    public async Task Validate_EmptyCustomerId_Fails()
    {
        // Arrange
        var validator = new UpdateDesiredVersionCommandValidator();
        var customerId = Guid.Empty;
        var desiredVersion = "1.0.0";
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(UpdateDesiredVersionCommand.CustomerId), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_EmptyDesiredVersion_Fails()
    {
        // Arrange
        var validator = new UpdateDesiredVersionCommandValidator();
        var customerId = Guid.NewGuid();
        var desiredVersion = string.Empty;
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(UpdateDesiredVersionCommand.DesiredVersion), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_InvalidDesiredVersionFormat_Fails()
    {
        // Arrange
        var validator = new UpdateDesiredVersionCommandValidator();
        var customerId = Guid.NewGuid();
        var desiredVersion = "1.0";
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(UpdateDesiredVersionCommand.DesiredVersion), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_DesiredVersionWithLetters_Fails()
    {
        // Arrange
        var validator = new UpdateDesiredVersionCommandValidator();
        var customerId = Guid.NewGuid();
        var desiredVersion = "1.0.A";
        var command = new UpdateDesiredVersionCommand(customerId, desiredVersion);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(UpdateDesiredVersionCommand.DesiredVersion), result.Errors[0].PropertyName);
    }
}