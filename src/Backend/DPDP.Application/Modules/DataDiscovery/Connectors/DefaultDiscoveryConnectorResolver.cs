using DPDP.Application.Common.Exceptions;
using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Application.Modules.DataDiscovery.Connectors;

public sealed class DefaultDiscoveryConnectorResolver(IEnumerable<IDiscoveryConnector> connectors) : IDiscoveryConnectorResolver
{
    public IDiscoveryConnector Resolve(DataSourceType sourceType) =>
        connectors.FirstOrDefault(c => c.SupportedType == sourceType)
            ?? throw new ConflictException($"No discovery connector is registered for source type {sourceType}.");
}
