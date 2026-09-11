using Xunit;

namespace DPDP.ApiTests.DataDiscovery;

[CollectionDefinition("DataDiscovery")]
public sealed class DataDiscoveryTestCollection : ICollectionFixture<DataDiscoveryApiFixture>;
