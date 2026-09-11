using DPDP.Application.Common.Interfaces;
using DPDP.Application.Modules.DataDiscovery.Connectors;
using DPDP.Domain.Modules.DataDiscovery;
using Microsoft.Extensions.Options;
using MediatR;

namespace DPDP.Application.Modules.DataDiscovery.Commands;

public sealed record TestDataSourceConnectionResultDto(bool Succeeded, string? ErrorMessage);

public sealed record TestDataSourceConnectionCommand(Guid Id) : IRequest<TestDataSourceConnectionResultDto>;

public sealed class TestDataSourceConnectionCommandHandler(
    IAppDbContext db, IConnectionSecretProtector secretProtector, IDiscoveryConnectorResolver connectorResolver,
    IOptions<DiscoveryOptions> discoveryOptions, IDateTimeProvider dateTimeProvider, IAuditLogger auditLogger)
    : IRequestHandler<TestDataSourceConnectionCommand, TestDataSourceConnectionResultDto>
{
    public async Task<TestDataSourceConnectionResultDto> Handle(TestDataSourceConnectionCommand request, CancellationToken cancellationToken)
    {
        var dataSource = await DataSourceLoader.LoadAsync(db, request.Id, cancellationToken);
        var connector = connectorResolver.Resolve(dataSource.SourceType);
        var connectionInfo = DataSourceConnectionInfoFactory.Build(dataSource, secretProtector, discoveryOptions.Value);

        var result = await connector.TestConnectionAsync(connectionInfo, cancellationToken);

        var now = dateTimeProvider.UtcNow;
        dataSource.LastTestedAt = now;
        dataSource.LastTestSucceeded = result.Succeeded;
        dataSource.LastTestError = result.ErrorMessage;
        await db.SaveChangesAsync(cancellationToken);

        await auditLogger.LogAsync(
            "datadiscovery.connection_tested", nameof(DataSource), dataSource.Id.ToString(),
            newValue: new { result.Succeeded }, cancellationToken: cancellationToken);

        return new TestDataSourceConnectionResultDto(result.Succeeded, result.ErrorMessage);
    }
}
