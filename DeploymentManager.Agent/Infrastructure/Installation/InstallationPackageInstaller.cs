using System.IO.Compression;
using DeploymentManager.Agent.Application.Features.Deployment.Interfaces;
using DeploymentManager.Agent.Application.Features.Deployment.Models;
using DeploymentManager.Agent.Application.Features.Deployment.Results;
using DeploymentManager.Agent.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace DeploymentManager.Agent.Infrastructure.Installation;

/// <summary>
/// Manages installation of the managed software.
/// </summary>
public sealed class InstallationPackageInstaller : IPackageInstaller
{
    private readonly InstallationOptions _options;
    private readonly ILogger<InstallationPackageInstaller> _logger;

    public InstallationPackageInstaller(IOptions<InstallationOptions> options, ILogger<InstallationPackageInstaller> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Extracts and installs the specified package, replacing any existing installation and supporting rollback on failure.
    /// </summary>
    public async Task<InstallationResult> InstallPackageAsync(InstallationPackage package, CancellationToken ct)
    {
        // Validation checks
        if (package is null)
        {
            _logger.LogWarning("Invalid package.");
            return InstallationResult.Failed;
        }

        var fileName = Path.GetFileName(package.FileName);
        
        if (string.IsNullOrWhiteSpace(fileName))
        {
            _logger.LogWarning("Invalid package filename {FileName}.", package.FileName);
            return InstallationResult.Failed;
        }

        if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Unsupported package format for file {FileName}.", package.FileName);
            return InstallationResult.Failed;
        }

        // Directory configuration
        var installDirectory = Path.GetFullPath(_options.InstallDirectory);
        var parentDirectory = Path.GetDirectoryName(installDirectory);
        
        if (string.IsNullOrWhiteSpace(parentDirectory))
        {
            _logger.LogWarning("Invalid directory {Directory}.", parentDirectory);
            return InstallationResult.Failed;
        }

        // Backup directory for rollback in case installation fails
        var backupDirectory = Path.Combine(parentDirectory, $".install-backup-{Guid.NewGuid()}");

        // Temporary directory for zip extraction
        var tempDirectory = Path.Combine(parentDirectory, $".install-temp-{Guid.NewGuid()}");
        var extractDirectory = Path.Combine(tempDirectory, "extract");
        var zipPath = Path.Combine(tempDirectory, fileName);

        // Flags for safe rollbacks in case of errors
        var backupCreated = false;
        var installCreated = false;

        try
        {
            Directory.CreateDirectory(parentDirectory);
            Directory.CreateDirectory(extractDirectory);

            // Copy zip content to new file at zip path
            await using (var fileStream = File.Create(zipPath))
            {
                package.Content.Position = 0;
                await package.Content.CopyToAsync(fileStream, ct);
            }

            await ExtractZipSafelyAsync(zipPath, extractDirectory, ct);

            if (!Directory.EnumerateFileSystemEntries(extractDirectory).Any())
            {
                _logger.LogWarning("Directory {Directory} empty after extraction.", extractDirectory);
                return InstallationResult.Failed;
            }

            // If an installation of the system already exists, copy files to backup,
            // keeping any existing files in installation directory
            if (Directory.Exists(installDirectory))
            {
                var installationFiles = Directory.GetFiles(installDirectory, "*", SearchOption.AllDirectories);
                foreach (var sourceFile in installationFiles)
                {
                    var relativeFilePath = Path.GetRelativePath(installDirectory, sourceFile);
                    var backupFile = Path.Combine(backupDirectory, relativeFilePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(backupFile)!);
                    File.Copy(sourceFile, backupFile, overwrite: true);
                }
                backupCreated = true;
            }
            else
            {
                Directory.CreateDirectory(installDirectory);
                installCreated = true;
            }

            // Select only files from extraction and ignore folders to avoid nesting
            var extractedFiles = Directory.EnumerateFiles(extractDirectory, "*", SearchOption.AllDirectories);

            // Copy extracted files individually to ensure configuration files
            // are not overwritten with defaults if exist
            foreach (var sourceFile in extractedFiles)
            {
                var name = Path.GetFileName(sourceFile);
                var destinationFile = Path.Combine(installDirectory, name);
                var isExecutable = name.EndsWith(".exec", StringComparison.OrdinalIgnoreCase);
                
                // Overwrite executable file
                if (isExecutable)
                {
                    File.Copy(sourceFile, destinationFile, overwrite: true);
                    continue;
                }

                // Copy file if does not exist
                if (!File.Exists(destinationFile))
                    File.Copy(sourceFile, destinationFile, overwrite: false);
            }

            // If backup directory exists, delete after successful installation
            if (Directory.Exists(backupDirectory))
                Directory.Delete(backupDirectory, recursive: true);

            _logger.LogInformation("Successfully installed package {FileName}.", package.FileName);
            return InstallationResult.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to install package {FileName}.", package.FileName);

            try
            {
                _logger.LogInformation("Attempting to roll back installation.");
                
                // Move backup directory contents to install directory
                if (backupCreated)
                {
                    if (Directory.Exists(installDirectory))
                        Directory.Delete(installDirectory, recursive: true);

                    Directory.Move(backupDirectory, installDirectory);
                    _logger.LogInformation("Successfully rolled back installation with backup.");
                }
                // Remove new install directory
                else if (installCreated && Directory.Exists(installDirectory))
                {
                    Directory.Delete(installDirectory, recursive: true);
                    _logger.LogInformation("Successfully rolled back installation.");
                }
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to roll back installation.");
            }
            return InstallationResult.Failed;
        }
        finally
        {
            try
            {
                if (Directory.Exists(tempDirectory))
                {
                    Directory.Delete(tempDirectory, recursive: true);
                    _logger.LogInformation("Successfully cleaned up temporary installation directory {Directory}.", tempDirectory);
                }
            }
            catch (Exception cleanupEx)
            {
                _logger.LogWarning(cleanupEx, "Failed to clean up temporary installation directory {Directory}.", tempDirectory);
            }
        }
    }



    /// <summary>
    /// Extracts zip contents safely while guarding against path traversal attacks.
    /// </summary>
    /// <param name="zipPath">Path to zip archive.</param>
    /// <param name="extractDirectory">Directory to extract archive to.</param>
    /// <param name="ct">Cancellation token for extraction.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when an entry tries to extract outside target directory.
    /// </exception>
    private async Task ExtractZipSafelyAsync(string zipPath, string extractDirectory, CancellationToken ct)
    {
        var extractPath = Path.GetFullPath(extractDirectory);
        
        // Ensures last character on extraction path is directory separator char
        if (!extractPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            extractPath += Path.DirectorySeparatorChar;
        
        using (ZipArchive archive = await ZipFile.OpenReadAsync(zipPath, ct))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                ct.ThrowIfCancellationRequested();

                // Gets full path to ensure relative segments are removed
                string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                if (!destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                {
                    _logger.LogWarning("Unsafe entry path: {Name}", entry.FullName);
                    throw new InvalidOperationException($"Unsafe entry path: {entry.FullName}");
                }
                
                // Create only directory when entry is a folder
                if (string.IsNullOrWhiteSpace(entry.Name))
                {
                    Directory.CreateDirectory(destinationPath);
                    continue;
                }

                // Create directory and file when entry is a file
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                entry.ExtractToFile(destinationPath, overwrite: true);
            }
        }
    }
}

// Sources:
// Directory: https://learn.microsoft.com/en-us/dotnet/api/system.io.directory?view=net-10.0
// File: https://learn.microsoft.com/en-us/dotnet/api/system.io.file?view=net-10.0
// ZipFile: https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.zipfile?view=net-10.0
// Path traversal attack protection: https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-compress-and-extract-files#example-1-create-and-extract-a-zip-file