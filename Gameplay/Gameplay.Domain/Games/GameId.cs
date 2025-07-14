namespace Gameplay.Domain.Games;

public sealed record GameId
{
    private GameId() { }

    public required string Value { get; init; }

    public static GameId New() => new() { Value = Guid.CreateVersion7().ToString() };
}
