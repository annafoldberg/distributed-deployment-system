using DeploymentManager.Api.Application.Features.InstallationPackages.Queries;

namespace DeploymentManager.Api.Tests.Application.Features.InstallationPackages.Queries;

[TestClass]
public sealed class GetInstallationPackageQueryValidatorTests
{
    [TestMethod]
    public async Task Validate_ValidAgentId_Succeeds()
    {
        // Arrange
        var validator = new GetInstallationPackageQueryValidator();
        var agentId = Guid.NewGuid();
        var query = new GetInstallationPackageQuery(agentId);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.HasCount(0, result.Errors);
    }

    [TestMethod]
    public async Task Validate_EmptyAgentId_Fails()
    {
        // Arrange
        var validator = new GetInstallationPackageQueryValidator();
        var agentId = Guid.Empty;
        var query = new GetInstallationPackageQuery(agentId);

        // Act
        var result = await validator.ValidateAsync(query);

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.AreEqual(nameof(GetInstallationPackageQuery.AgentId), result.Errors[0].PropertyName);
    }
}