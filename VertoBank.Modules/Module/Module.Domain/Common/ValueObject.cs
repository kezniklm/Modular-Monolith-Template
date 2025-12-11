namespace Module.Domain.Common;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public bool Equals(ValueObject? other) => Equals((object?)other);

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode() =>
        GetEqualityComponents()
            .Select(x => x != null ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);

    public static bool operator ==(ValueObject one, ValueObject two) => EqualOperator(one, two);

    public static bool operator !=(ValueObject one, ValueObject two) => NotEqualOperator(one, two);

    protected abstract IEnumerable<object?> GetEqualityComponents();

    protected static bool EqualOperator(ValueObject? left, ValueObject? right)
    {
        if (left is null || right is null)
        {
            return false;
        }

        return ReferenceEquals(left, right) || left.Equals(right);
    }

    protected static bool NotEqualOperator(ValueObject left, ValueObject right) => !EqualOperator(left, right);
}
