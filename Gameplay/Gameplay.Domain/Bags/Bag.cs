using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Bags;

public sealed class Bag : IBag
{
    private readonly List<Tile> _tilesInBag = [];

    private Bag() { }

    public int TilesCount => _tilesInBag.Count;

    public void AddTiles(IEnumerable<Tile> tiles) => _tilesInBag.AddRange(tiles);

    public void ShuffleTiles() => throw new NotImplementedException();

    public IEnumerable<Tile> DrawTiles(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        var tilesToDraw = Math.Min(count, TilesCount);

        for (var i = 0; i < tilesToDraw; i++)
        {
            yield return _tilesInBag[i];
        }

        _tilesInBag.RemoveRange(0, tilesToDraw);
    }
}
