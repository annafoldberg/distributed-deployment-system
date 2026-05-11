using MediatR;
using FluentResults;

namespace DeploymentManager.Api.Application.Features.Deployments.Commands;

/// <summary>
/// Command for reporting the result of an installation attempt for an agent.
/// </summary>
public sealed record ReportInstallationResultCommand(Guid AgentId, bool Succeeded,
    string? InstalledVersion, string? ErrorMessage) : IRequest<Result>;