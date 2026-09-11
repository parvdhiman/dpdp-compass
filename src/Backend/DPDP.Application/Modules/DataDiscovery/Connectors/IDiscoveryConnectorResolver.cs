using DPDP.Domain.Modules.DataDiscovery;

namespace DPDP.Application.Modules.DataDiscovery.Connectors;

public interface IDiscoveryConnectorResolver
{
    IDiscoveryConnector Resolve(DataSourceType sourceType);
}
