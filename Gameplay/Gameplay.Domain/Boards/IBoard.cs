using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Boards;

public interface IBoard
{
    void SetTile(Tile tile, BoardCellPosition position);

    BoardCell GetCell(BoardCellPosition position);
}
