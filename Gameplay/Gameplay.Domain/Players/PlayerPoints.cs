namespace Gameplay.Domain.Players;

public sealed record PlayerPoints
{
    public static readonly PlayerPoints Initial = new() { Value = 0 };

    private PlayerPoints() { }

    public required int Value { get; init; }
}
