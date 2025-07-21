namespace Gameplay.Domain.Settings;

public sealed record RackCapacity
{
    private const int MinValue = 2;
    private const int MaxValue = 30;

    private RackCapacity() { }

    public required int Value { get; init; }

    public static RackCapacity From(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxValue);
        return new RackCapacity { Value = value };
    }
}
