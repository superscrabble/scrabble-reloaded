using System.Collections.Immutable;

namespace Gameplay.Domain.Games;

public sealed record ValidWords
{
    private ValidWords() { }

    public required int TotalPoints { get; init; }


    public required ImmutableArray<TilePositionModel> TilePositions { get; init; }
}
