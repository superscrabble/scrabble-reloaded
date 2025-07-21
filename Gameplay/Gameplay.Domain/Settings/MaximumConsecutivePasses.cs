namespace Gameplay.Domain.Settings;

public sealed record MaximumConsecutivePasses
{
    private const int MinValue = 1;
    private const int MaxValue = 20;

    private MaximumConsecutivePasses() { }

    public required int Value { get; init; }

    public static MaximumConsecutivePasses From(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxValue);
        return new MaximumConsecutivePasses { Value = value };
    }
}
