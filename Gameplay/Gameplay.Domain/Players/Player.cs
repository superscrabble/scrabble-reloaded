using System.Collections.Immutable;

using Gameplay.Domain.Bags;
using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Players;

public sealed class Player : IPlayer
{
    private readonly List<Tile> _tilesOnRack = [];

    private Player(
        PlayerId id,
        RackCapacity rackCapacity)
    {
        Id = id;
        RackCapacity = rackCapacity;
    }

    public PlayerId Id { get; }

    public RackCapacity RackCapacity { get; }

    public ConsecutivePasses ConsecutivePasses { get; private set; } = ConsecutivePasses.Initial;

    public PlayerPoints Points { get; } = PlayerPoints.Initial;

    public bool HasSurrendered { get; private set; }

    public void ResetConsecutivePassesCount()
    {
        ThrowIfPlayerHasSurrendered();
        ConsecutivePasses = ConsecutivePasses.Initial;
    }

    public void IncrementConsecutivePasses()
    {
        ThrowIfPlayerHasSurrendered();
        ConsecutivePasses = ConsecutivePasses.Increment();
    }

    public void Surrender() => HasSurrendered = true;

    public void AddTilesToRack(ImmutableArray<Tile> tiles)
    {
        ThrowIfPlayerHasSurrendered();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_tilesOnRack.Count + tiles.Length, RackCapacity.Value);
        _tilesOnRack.AddRange(tiles);
    }

    public void ReturnTilesToBag(ImmutableArray<Tile> tiles, IBag bag)
    {
        ThrowIfPlayerHasSurrendered();
        ThrowIfPlayerDoesNotOwnTiles(tiles);
        RemoveTilesFromRack(tiles);
        bag.AddTiles(tiles);
    }

    private void ThrowIfPlayerDoesNotOwnTiles(IEnumerable<Tile> tiles)
    {
        var playerTilesOnRackCopy = _tilesOnRack.ToList();

        foreach (var tileToExchange in tiles)
        {
            var tileRemoved = playerTilesOnRackCopy.Remove(tileToExchange);

            if (!tileRemoved)
            {
                throw new PlayerDoesNotOwnTileException(Id);
            }
        }
    }

    private void RemoveTilesFromRack(IEnumerable<Tile> tilesToExchange)
    {
        foreach (var tileToExchange in tilesToExchange)
        {
            _tilesOnRack.Remove(tileToExchange);
        }
    }

    private void ThrowIfPlayerHasSurrendered()
    {
        if (HasSurrendered)
        {
            throw new PlayerHasSurrenderedException(Id);
        }
    }
}
