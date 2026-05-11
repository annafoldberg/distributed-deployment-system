using Moq;
using Microsoft.Extensions.Logging;
using DeploymentManager.Agent.Application.Features.Deployment.Interfaces;
using DeploymentManager.Agent.Application.Features.Deployment;
using DeploymentManager.Agent.Application.Features.Deployment.Models;
using DeploymentManager.Agent.Application.Features.Deployment.Results;
using DeploymentManager.Agent.Infrastructure.Installation;
using DeploymentManager.Agent.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.IO.Compression;

namespace DeploymentManager.Agent.Tests.Application.Features.Deployment;

[TestClass]
public sealed class InstallationPackageInstallerTests
{
    private string _testRoot = string.Empty;
    private string _installDirectory = string.Empty;
    private InstallationPackageInstaller _installer = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), "installer-tests", Guid.NewGuid().ToString());
        var subDirectory = "TestSub";
        var appDirectory = "TestApp";
        _installDirectory = Path.Combine(_testRoot, subDirectory, appDirectory);

        var options = Options.Create(new InstallationOptions
        {
            Root = _testRoot,
            SubDirectory = subDirectory,
            AppDirectory = appDirectory
        });

        var logger = Mock.Of<ILogger<InstallationPackageInstaller>>();
        _installer = new InstallationPackageInstaller(options, logger);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testRoot))
            Directory.Delete(_testRoot, recursive: true);
    }

    [TestMethod]
    public async Task InstallPackageAsync_ValidZipPackage_ReturnsSucceeded()
    {
        // Arrange
        var execFile = ("artifacts/osx-arm64/UpdatableApp.exec", "executable content");
        var installationPackage = CreatePackage(new[] { execFile });
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Succeeded, result);
    }

    [TestMethod]
    public async Task InstallPackageAsync_ValidZipPackage_CreatesInstallDirectory()
    {
        // Arrange
        var execFile = ("artifacts/osx-arm64/UpdatableApp.exec", "executable content");
        var installationPackage = CreatePackage(new[] { execFile });
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Succeeded, result);
        Assert.IsTrue(Directory.Exists(_installDirectory));
    }

    [TestMethod]
    public async Task InstallPackageAsync_ValidZipPackage_CopiesExecutableToInstallDirectory()
    {
        // Arrange
        var execFile = ("artifacts/osx-arm64/UpdatableApp.exec", "executable content");
        var installationPackage = CreatePackage(new[] { execFile });
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Succeeded, result);
        var file = Path.Combine(_installDirectory, "UpdatableApp.exec");
        Assert.IsTrue(File.Exists(file));
        var content = await File.ReadAllTextAsync(file);
        Assert.AreEqual("executable content", content);
    }

    [TestMethod]
    public async Task InstallPackageAsync_FileIsExecutable_OverwritesExistingFile()
    {
        // Arrange
        Directory.CreateDirectory(_installDirectory);
        var existingExecFile = Path.Combine(_installDirectory, "UpdatableApp.exec");
        await File.WriteAllTextAsync(existingExecFile, "old executable content");
        
        var newExecFile = ("artifacts/osx-arm64/UpdatableApp.exec", "new executable content");
        var installationPackage = CreatePackage(new[] { newExecFile });
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Succeeded, result);
        Assert.IsTrue(Directory.Exists(_installDirectory));
        var file = Path.Combine(_installDirectory, "UpdatableApp.exec");
        Assert.IsTrue(File.Exists(file));
        var content = await File.ReadAllTextAsync(file);
        Assert.AreEqual("new executable content", content);
    }

    [TestMethod]
    public async Task InstallPackageAsync_AppsettingsExist_DoesNotOverwriteFile()
    {
        // Arrange
        Directory.CreateDirectory(_installDirectory);
        var existingAppsettings = Path.Combine(_installDirectory, "appsettings.json");
        await File.WriteAllTextAsync(existingAppsettings, "old appsettings content");
        
        var newAppsettings = ("artifacts/appsettings.json", "new appsettings content");
        var installationPackage = CreatePackage(new[] { newAppsettings });
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Succeeded, result);
        Assert.IsTrue(Directory.Exists(_installDirectory));
        var file = Path.Combine(_installDirectory, "appsettings.json");
        Assert.IsTrue(File.Exists(file));
        var content = await File.ReadAllTextAsync(file);
        Assert.AreEqual("old appsettings content", content);
    }

    [TestMethod]
    public async Task InstallPackageAsync_AppsettingsDoNotExist_CopiesDefaultAppsettings()
    {
        // Arrange
        var appsettings = ("artifacts/appsettings.json", "appsettings content");
        var installationPackage = CreatePackage(new[] { appsettings });
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Succeeded, result);
        Assert.IsTrue(Directory.Exists(_installDirectory));
        var file = Path.Combine(_installDirectory, "appsettings.json");
        Assert.IsTrue(File.Exists(file));
        var content = await File.ReadAllTextAsync(file);
        Assert.AreEqual("appsettings content", content);
    }

    [TestMethod]
    public async Task InstallPackageAsync_PackageIsNull_ReturnsFailed()
    {        
        // Act
        var result = await _installer.InstallPackageAsync(null!, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Failed, result);
        Assert.IsFalse(Directory.Exists(_installDirectory));
    }

    [TestMethod]
    public async Task InstallPackageAsync_PackageIsNotZip_ReturnsFailed()
    {
        // Arrange
        var stream = new MemoryStream();
        var installationPackage = new InstallationPackage(stream, "package.txt");
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Failed, result);
        Assert.IsFalse(Directory.Exists(_installDirectory));
    }

    [TestMethod]
    public async Task InstallPackageAsync_InvalidPackageFileName_ReturnsFailed()
    {
        // Arrange
        var stream = new MemoryStream();
        var installationPackage = new InstallationPackage(stream, "");
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Failed, result);
        Assert.IsFalse(Directory.Exists(_installDirectory));
    }

    [TestMethod]
    public async Task InstallPackageAsync_ZipEntryWithPathTraversal_DoesNotWriteOutsideInstallDirectory()
    {
        // Arrange
        var badFile = ("../../path-traversal-test.txt", "file content");
        var installationPackage = CreatePackage(new[] { badFile });
        var outsideDirectoryFile = Path.Combine(_testRoot, "TestSub", "path-traversal-test.txt");
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Failed, result);
        Assert.IsFalse(Directory.Exists(_installDirectory));
        Assert.IsFalse(File.Exists(outsideDirectoryFile));
    }

    [TestMethod]
    public async Task InstallPackageAsync_InstallationFailsWithExistingInstallation_RestoresPreviousInstallationFromBackup()
    {
        // Arrange
        Directory.CreateDirectory(_installDirectory);
        var existingExecFile = Path.Combine(_installDirectory, "UpdatableApp.exec");
        await File.WriteAllTextAsync(existingExecFile, "old executable content");

        var badFile = ("../../path-traversal-test.txt", "file content");
        var installationPackage = CreatePackage(new[] { badFile });
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Failed, result);
        Assert.IsTrue(Directory.Exists(_installDirectory));
        var file = Path.Combine(_installDirectory, "UpdatableApp.exec");
        Assert.IsTrue(File.Exists(file));
        var content = await File.ReadAllTextAsync(file);
        Assert.AreEqual("old executable content", content);
    }

    [TestMethod]
    public async Task InstallPackageAsync_InstallationFailsWithNoExistingInstallation_RemovesCreatedInstallDirectory()
    {
        // Arrange
        var stream = new MemoryStream();
        var installationPackage = new InstallationPackage(stream, "package.txt");
        
        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Failed, result);
        Assert.IsFalse(Directory.Exists(_installDirectory));
    }

    [TestMethod]
    public async Task InstallPackageAsync_InstallationSucceeds_DeletesTemporaryDirectory()
    {
        // Arrange
        var execFile = ("artifacts/osx-arm64/UpdatableApp.exec", "executable content");
        var installationPackage = CreatePackage(new[] { execFile });
        
        var parentDirectory = Path.GetDirectoryName(_installDirectory);

        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Succeeded, result);
        var tempDirectories = Directory.Exists(parentDirectory!)
                              ? Directory.GetDirectories(parentDirectory!, ".install-temp-*")
                              : [];
        Assert.IsEmpty(tempDirectories);
    }

    [TestMethod]
    public async Task InstallPackageAsync_InstallationFails_DeletesTemporaryDirectory()
    {
        // Arrange
        var stream = new MemoryStream();
        var installationPackage = new InstallationPackage(stream, "package.txt");
        
        var parentDirectory = Path.GetDirectoryName(_installDirectory);

        // Act
        var result = await _installer.InstallPackageAsync(installationPackage, CancellationToken.None);

        // Assert
        Assert.AreEqual(InstallationResult.Failed, result);
        var tempDirectories = Directory.Exists(parentDirectory!)
                              ? Directory.GetDirectories(parentDirectory!, ".install-temp-*")
                              : [];
        Assert.IsEmpty(tempDirectories);
    }

    // -------------------- Helper Methods --------------------
    private InstallationPackage CreatePackage((string path, string content)[] files)
    {
        var stream = new MemoryStream();
        
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.path);
                using var writer = new StreamWriter(entry.Open());
                writer.Write(file.content);
            }
        }

        stream.Position = 0;
        return new InstallationPackage(stream, "package.zip");
    }
}

// Source: https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchive?view=net-10.0