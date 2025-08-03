using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Boards;

public sealed record BoardCell
{
    private BoardCell() { }

    public required Tile? Tile { get; init; }

    public required BoardCellType Type { get; init; }
}
