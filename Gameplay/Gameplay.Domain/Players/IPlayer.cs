using System.Collections.Immutable;

using Gameplay.Domain.Bags;
using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Players;

public interface IPlayer
{
    PlayerId Id { get; }

    RackCapacity RackCapacity { get; }

    ConsecutivePasses ConsecutivePasses { get; }

    PlayerPoints Points { get; }

    bool HasSurrendered { get; }

    void IncrementConsecutivePasses();

    void ResetConsecutivePassesCount();

    void Surrender();

    void AddTilesToRack(ImmutableArray<Tile> tiles);

    void ReturnTilesToBag(ImmutableArray<Tile> tiles, IBag bag);
}
