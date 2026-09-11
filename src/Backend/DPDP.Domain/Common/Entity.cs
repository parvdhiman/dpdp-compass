namespace DPDP.Domain.Common;

/// <summary>
/// Base type for every entity in the domain model. Identity equality only —
/// two entities are equal iff they are the same CLR type and share an Id.
/// </summary>
public abstract class Entity : IEquatable<Entity>
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public bool Equals(Entity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetType() == other.GetType() && Id == other.Id;
    }

    public override bool Equals(object? obj) => Equals(obj as Entity);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity? left, Entity? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity? left, Entity? right) => !(left == right);
}
