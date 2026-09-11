using DPDP.Application.Common.Exceptions;
using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.DTOs;
using DPDP.Domain.Modules.DataDiscovery;
using FluentValidation;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Commands;

public sealed record CreateDataSourceCommand(
    string Name, string? Description, string SourceType,
    string? Host, int? Port, string? DatabaseName, string? Username, string? Password,
    string? RootPath, string? SchemaFilter) : IRequest<DataSourceDetailDto>;

public sealed class CreateDataSourceCommandValidator : AbstractValidator<CreateDataSourceCommand>
{
    public CreateDataSourceCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.SourceType)
            .Must(v => Enum.TryParse<DataSourceType>(v, out _))
            .WithMessage("sourceType must be one of: " + string.Join(", ", Enum.GetNames<DataSourceType>()));

        RuleFor(x => x.RootPath)
            .NotEmpty()
            .When(x => x.SourceType == nameof(DataSourceType.FILE_SYSTEM))
            .WithMessage("rootPath is required for a FILE_SYSTEM data source.");

        RuleFor(x => x.Host)
            .NotEmpty()
            .When(x => x.SourceType != nameof(DataSourceType.FILE_SYSTEM))
            .WithMessage("host is required for a database data source.");
        RuleFor(x => x.DatabaseName)
            .NotEmpty()
            .When(x => x.SourceType != nameof(DataSourceType.FILE_SYSTEM))
            .WithMessage("databaseName is required for a database data source.");
        RuleFor(x => x.Username)
            .NotEmpty()
            .When(x => x.SourceType != nameof(DataSourceType.FILE_SYSTEM))
            .WithMessage("username is required for a database data source.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .When(x => x.SourceType != nameof(DataSourceType.FILE_SYSTEM))
            .WithMessage("password is required for a database data source.");
        RuleFor(x => x.Port).InclusiveBetween(1, 65535).When(x => x.Port.HasValue);
    }
}

public sealed class CreateDataSourceCommandHandler(
    IAppDbContext db, ICurrentUserContext currentUser,
    IConnectionSecretProtector secretProtector, IAuditLogger auditLogger)
    : IRequestHandler<CreateDataSourceCommand, DataSourceDetailDto>
{
    public async Task<DataSourceDetailDto> Handle(CreateDataSourceCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.OrganisationId is not { } organisationId)
        {
            throw new ForbiddenException("Only a member of an organisation can register a data source.");
        }

        var sourceType = Enum.Parse<DataSourceType>(request.SourceType);

        var dataSource = new DataSource
        {
            OrganisationId = organisationId,
            Name = request.Name.Trim(),
            Description = request.Description,
            SourceType = sourceType,
            Host = request.Host,
            Port = request.Port,
            DatabaseName = request.DatabaseName,
            Username = request.Username,
            EncryptedSecret = request.Password is null ? null : secretProtector.Protect(request.Password),
            RootPath = request.RootPath,
            SchemaFilter = request.SchemaFilter,
            IsActive = true,
        };

        db.DataSources.Add(dataSource);
        await db.SaveChangesAsync(cancellationToken);
        await auditLogger.LogAsync(
            "datadiscovery.datasource_created", nameof(DataSource), dataSource.Id.ToString(),
            newValue: new { dataSource.Name, dataSource.SourceType }, cancellationToken: cancellationToken);

        return DataDiscoveryMapper.ToDetailDto(dataSource);
    }
}
