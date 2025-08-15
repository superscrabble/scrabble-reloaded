namespace Gameplay.Domain.Games;

public sealed record MaximumTotalGameTurns
{
    private const int MinValue = 10;
    private const int MaxValue = 400;

    private MaximumTotalGameTurns() { }

    public required int Value { get; init; }

    public static MaximumTotalGameTurns From(int value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, MinValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaxValue);
        return new MaximumTotalGameTurns { Value = value };
    }
}
