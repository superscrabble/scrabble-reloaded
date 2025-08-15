using System.Collections.Immutable;

using Gameplay.Domain.Boards;
using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Words;

public interface IValidWords
{
    int TotalPoints { get; }

    ImmutableArray<Tile> Tiles { get; }

    ImmutableDictionary<Tile, BoardCellPosition> TilePositions { get; }
}
