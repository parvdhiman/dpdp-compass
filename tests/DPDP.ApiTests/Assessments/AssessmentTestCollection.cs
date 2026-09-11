using Xunit;

namespace DPDP.ApiTests.Assessments;

[CollectionDefinition("Assessments")]
public sealed class AssessmentTestCollection : ICollectionFixture<AssessmentApiFixture>;
