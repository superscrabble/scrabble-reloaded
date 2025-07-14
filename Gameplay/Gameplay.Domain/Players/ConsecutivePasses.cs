namespace Gameplay.Domain.Players;

public sealed record ConsecutivePasses
{
    public static readonly ConsecutivePasses Initial = new() { Value = 0 };

    private ConsecutivePasses() { }

    public required int Value { get; init; }

    public ConsecutivePasses Increment() => new() { Value = Value + 1 };
}
