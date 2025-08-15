using System.Collections.Immutable;

using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Players;

public sealed class Player : IPlayer
{
    private readonly List<Tile> _tilesOnRack = [];

    private Player(
        PlayerId id,
        RackCapacity rackCapacity,
        ConsecutivePasses consecutivePasses,
        int score,
        bool hasSurrendered)
    {
        Id = id;
        RackCapacity = rackCapacity;
        ConsecutivePasses = consecutivePasses;
        Score = score;
        HasSurrendered = hasSurrendered;
    }

    public PlayerId Id { get; }

    public RackCapacity RackCapacity { get; }

    public ConsecutivePasses ConsecutivePasses { get; private set; }

    public int Score { get; private set; }

    public bool HasAnyTiles => _tilesOnRack.Count > 0;

    public bool HasSurrendered { get; private set; }

    public int RemainingTilesPoints => _tilesOnRack.Sum(t => t.Points);

    public void Surrender() => HasSurrendered = true;

    public void IncrementPoints(int points)
    {
        ThrowIfPlayerHasSurrendered();
        Score += points;
    }

    public void SubtractRemainingTilesPointsFromScore()
    {
        ThrowIfPlayerHasSurrendered();
        Score -= RemainingTilesPoints;
    }

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

    public void AddTilesToRack(ImmutableArray<Tile> tiles)
    {
        ThrowIfPlayerHasSurrendered();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(_tilesOnRack.Count + tiles.Length, RackCapacity.Value);
        _tilesOnRack.AddRange(tiles);
    }

    public void RemoveTilesFromRack(ImmutableArray<Tile> tiles)
    {
        ThrowIfPlayerDoesNotOwnTiles(tiles);

        foreach (var tile in tiles)
        {
            _tilesOnRack.Remove(tile);
        }
    }

    private void ThrowIfPlayerHasSurrendered()
    {
        if (HasSurrendered)
        {
            throw new PlayerHasSurrenderedException(Id);
        }
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
}
