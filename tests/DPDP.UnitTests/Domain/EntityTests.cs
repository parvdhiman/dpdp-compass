using DPDP.Domain.Common;
using Xunit;

namespace DPDP.UnitTests.Domain;

public class EntityTests
{
    private sealed class SampleEntity : Entity;

    private sealed class OtherEntity : Entity;

    [Fact]
    public void An_entity_equals_itself()
    {
        var entity = new SampleEntity();
        var sameReference = entity;

        Assert.Equal(entity, sameReference);
        Assert.True(entity == sameReference);
    }

    [Fact]
    public void Two_new_entities_get_distinct_ids_and_are_not_equal()
    {
        var a = new SampleEntity();
        var b = new SampleEntity();

        Assert.NotEqual(a.Id, b.Id);
        Assert.NotEqual(a, b);
    }

    [Fact]
    public void An_entity_is_never_equal_to_null()
    {
        var entity = new SampleEntity();

        Assert.False(entity.Equals(null));
        Assert.False(entity == null);
        Assert.True(entity != null);
    }
}
