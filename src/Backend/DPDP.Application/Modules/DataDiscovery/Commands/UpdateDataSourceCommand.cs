using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Commands;

internal static class DataSourceLoader
{
    public static async Task<DataSource> LoadAsync(IAppDbContext db, Guid id, CancellationToken cancellationToken) =>
        await db.DataSources.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(DataSource), id);
}

/// <summary>
/// Password is optional here — omitting it (null) leaves the existing
/// encrypted secret untouched, the same "rotate only if provided"
/// semantics as a typical credential-update form. There is no way to
/// clear a secret back to empty short of deleting the DataSource; that is
/// deliberate, not an oversight.
/// </summary>
public sealed record UpdateDataSourceCommand(
    Guid Id, string Name, string? Description,
    string? Host, int? Port, string? DatabaseName, string? Username, string? Password,
    string? RootPath, string? SchemaFilter, bool IsActive) : IRequest<DataSourceDetailDto>;

public sealed class UpdateDataSourceCommandValidator : AbstractValidator<UpdateDataSourceCommand>
{
    public UpdateDataSourceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).When(x => x.Port.HasValue);
    }
}

public sealed class UpdateDataSourceCommandHandler(IAppDbContext db, IConnectionSecretProtector secretProtector, IAuditLogger auditLogger)
    : IRequestHandler<UpdateDataSourceCommand, DataSourceDetailDto>
{
    public async Task<DataSourceDetailDto> Handle(UpdateDataSourceCommand request, CancellationToken cancellationToken)
    {
        var dataSource = await DataSourceLoader.LoadAsync(db, request.Id, cancellationToken);

        dataSource.Name = request.Name.Trim();
        dataSource.Description = request.Description;
        dataSource.Host = request.Host;
        dataSource.Port = request.Port;
        dataSource.DatabaseName = request.DatabaseName;
        dataSource.Username = request.Username;
        dataSource.RootPath = request.RootPath;
        dataSource.SchemaFilter = request.SchemaFilter;
        dataSource.IsActive = request.IsActive;
        if (!string.IsNullOrEmpty(request.Password))
        {
            dataSource.EncryptedSecret = secretProtector.Protect(request.Password);
        }

        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync("datadiscovery.datasource_updated", nameof(DataSource), dataSource.Id.ToString(), cancellationToken: cancellationToken);

        return DataDiscoveryMapper.ToDetailDto(dataSource);
    }
}
