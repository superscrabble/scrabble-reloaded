using System.Collections.Immutable;

using Gameplay.Domain.Boards;
using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Words;

public sealed record ValidWords : IValidWords
{
    private ValidWords() { }

    public required int TotalPoints { get; init; }

    public required ImmutableArray<Tile> Tiles { get; init; }

    public required ImmutableDictionary<Tile, BoardCellPosition> TilePositions { get; init; }
}
