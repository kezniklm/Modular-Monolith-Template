namespace Module.Domain.Common;

public abstract class Entity<TId> : BaseEntity, IEquatable<Entity<TId>> where TId : notnull
{
    protected Entity() { }

    protected Entity(TId id)
    {
        if (Equals(id, default(TId)))
        {
            throw new ArgumentException("The ID cannot be the default value.", nameof(id));
        }

        Id = id;
    }

    public TId Id { get; protected set; } = default!;

    public bool Equals(Entity<TId>? other) => Equals((object?)other);

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Id.Equals(entity.Id);

    public static bool operator ==(Entity<TId> one, Entity<TId> two) => Equals(one, two);

    public static bool operator !=(Entity<TId> one, Entity<TId> two) => !Equals(one, two);

    public override int GetHashCode() => Id.GetHashCode();
}
