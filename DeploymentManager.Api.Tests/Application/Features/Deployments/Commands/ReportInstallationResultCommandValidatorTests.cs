using DeploymentManager.Api.Application.Features.Deployments.Commands;
using FluentResults;

namespace DeploymentManager.Api.Tests.Application.Features.Deployments.Commands;

[TestClass]
public sealed class ReportInstallationResultCommandValidatorTests
{
    [TestMethod]
    public async Task Validate_ValidAgentId_Succeeds()
    {
        // Arrange
        var validator = new ReportInstallationResultCommandValidator();
        var agentId = Guid.NewGuid();
        var succeeded = true;
        var installedVersion = "1.0.0";
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, null);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.HasCount(0, result.Errors);
    }

    [TestMethod]
    public async Task Validate_EmptyAgentId_Fails()
    {
        // Arrange
        var validator = new ReportInstallationResultCommandValidator();
        var agentId = Guid.Empty;
        var succeeded = true;
        var installedVersion = "1.0.0";
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, null);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(ReportInstallationResultCommand.AgentId), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_InvalidInstalledVersionFormat_Fails()
    {
        // Arrange
        var validator = new ReportInstallationResultCommandValidator();
        var agentId = Guid.NewGuid();
        var succeeded = true;
        var installedVersion = "1.0";
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, null);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(ReportInstallationResultCommand.InstalledVersion), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_InstalledVersionWithLetters_Fails()
    {
        // Arrange
        var validator = new ReportInstallationResultCommandValidator();
        var agentId = Guid.NewGuid();
        var succeeded = true;
        var installedVersion = "1.0.A";
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, null);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(ReportInstallationResultCommand.InstalledVersion), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_InstalledVersionNotNullWhenSucceedsFalse_Fails()
    {
        // Arrange
        var validator = new ReportInstallationResultCommandValidator();
        var agentId = Guid.NewGuid();
        var succeeded = false;
        var installedVersion = "1.0.0";
        var errorMessage = "Error";
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, errorMessage);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(ReportInstallationResultCommand.InstalledVersion), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_InstalledVersionNullWhenSucceedsTrue_Fails()
    {
        // Arrange
        var validator = new ReportInstallationResultCommandValidator();
        var agentId = Guid.NewGuid();
        var succeeded = true;
        var command = new ReportInstallationResultCommand(agentId, succeeded, null, null);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(ReportInstallationResultCommand.InstalledVersion), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_ErrorMessageNotNullWhenSucceedsTrue_Fails()
    {
        // Arrange
        var validator = new ReportInstallationResultCommandValidator();
        var agentId = Guid.NewGuid();
        var succeeded = true;
        var installedVersion = "1.0.0";
        var errorMessage = "Error";
        var command = new ReportInstallationResultCommand(agentId, succeeded, installedVersion, errorMessage);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(ReportInstallationResultCommand.ErrorMessage), result.Errors[0].PropertyName);
    }

    [TestMethod]
    public async Task Validate_ErrorMessageNullWhenSucceedsFalse_Fails()
    {
        // Arrange
        var validator = new ReportInstallationResultCommandValidator();
        var agentId = Guid.NewGuid();
        var succeeded = false;
        var command = new ReportInstallationResultCommand(agentId, succeeded, null, null);

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(ReportInstallationResultCommand.ErrorMessage), result.Errors[0].PropertyName);
    }
}