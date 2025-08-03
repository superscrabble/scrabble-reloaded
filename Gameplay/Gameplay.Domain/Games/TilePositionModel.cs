using Gameplay.Domain.Boards;
using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Games;

public sealed record TilePositionModel(Tile Tile, BoardCellPosition Position);
