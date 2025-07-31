using System.Collections.Immutable;

using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Games;

public sealed record WrittenWords
{
    private WrittenWords() { }

    public required int TotalPoints { get; init; }

    public required ImmutableArray<Tile> UsedPlayerTiles { get; init; }
}
