using System.Collections.Immutable;

using Gameplay.Domain.Settings;
using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Players;

public interface IPlayer
{
    PlayerId Id { get; }

    RackCapacity RackCapacity { get; }

    ConsecutivePasses ConsecutivePasses { get; }

    int Score { get; }

    bool HasAnyTiles { get; }

    bool HasSurrendered { get; }

    int RemainingTilesPoints { get; }

    void Surrender();

    void IncrementPoints(int points);

    void SubtractRemainingTilesPointsFromScore();

    void IncrementConsecutivePasses();

    void ResetConsecutivePassesCount();

    void AddTilesToRack(ImmutableArray<Tile> tiles);

    void RemoveTilesFromRack(ImmutableArray<Tile> tiles);
}
