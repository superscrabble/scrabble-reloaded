namespace Gameplay.Domain.Players;

public sealed record PlayerId
{
    private PlayerId() { }

    public required string Value { get; init; }

    public static PlayerId New() => new() { Value = Guid.CreateVersion7().ToString() };
}
