using Xunit;

namespace DPDP.ApiTests.DataInventory;

[CollectionDefinition("DataInventory")]
public sealed class DataInventoryTestCollection : ICollectionFixture<DataInventoryApiFixture>;
