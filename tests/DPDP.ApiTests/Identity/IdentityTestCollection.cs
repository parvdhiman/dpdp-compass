using Xunit;

namespace DPDP.ApiTests.Identity;

[CollectionDefinition("Identity")]
public sealed class IdentityTestCollection : ICollectionFixture<IdentityApiFixture>;
