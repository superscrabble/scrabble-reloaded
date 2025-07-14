using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Bags;

public interface IBag
{
    int TilesCount { get; }

    void AddTiles(IEnumerable<Tile> tiles);

    void ShuffleTiles();

    IEnumerable<Tile> DrawTiles(int count);
}
