using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Bags;

public sealed class Bag : IBag
{
    private readonly List<Tile> _tilesInBag = [];
    private readonly IShuffler _shuffler;

    private Bag(IShuffler shuffler) => _shuffler = shuffler;

    public int TilesCount => _tilesInBag.Count;

    public void AddTiles(IEnumerable<Tile> tiles) => _tilesInBag.AddRange(tiles);

    public void ShuffleTiles() => _shuffler.Shuffle(_tilesInBag);

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

    public static Bag Create(IShuffler shuffler, IEnumerable<Tile> tiles)
    {
        var bag = new Bag(shuffler);
        bag.AddTiles(tiles);
        return bag;
    }
}
