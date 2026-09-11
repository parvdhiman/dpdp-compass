using Xunit;

namespace DPDP.ApiTests.Evidence;

[CollectionDefinition("Evidence")]
public sealed class EvidenceTestCollection : ICollectionFixture<EvidenceApiFixture>;
