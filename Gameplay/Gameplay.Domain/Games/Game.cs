using System.Collections.Immutable;

using Gameplay.Domain.Bags;
using Gameplay.Domain.Players;
using Gameplay.Domain.Settings;
using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Games;

public sealed class Game
{
    private const int MinimumPlayersCount = 2;
    private const int MaximumPlayersCount = 4;

    private readonly IBag _bag;
    private readonly MaximumConsecutivePasses _maximumConsecutivePasses;
    private readonly List<IPlayer> _players;

    private IPlayer _currentPlayer;

    private Game(
        GameId id,
        IBag bag,
        List<IPlayer> players,
        IPlayer currentPlayer,
        MaximumConsecutivePasses maximumConsecutivePasses)
    {
        Id = id;
        _bag = bag;

        var distinctPlayersCount = players.Select(p => p.Id).Distinct().Count();
        ArgumentOutOfRangeException.ThrowIfNotEqual(players.Count, distinctPlayersCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(players.Count, MinimumPlayersCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(players.Count, MaximumPlayersCount);
        _players = players;

        if (!players.Contains(currentPlayer))
        {
            throw new CurrentPlayerIsMissingException(id);
        }

        _currentPlayer = currentPlayer;
        _maximumConsecutivePasses = maximumConsecutivePasses;
    }

    private GameId Id { get; }

    public bool HasEnded { get; private set; }

    public ImmutableArray<PlayerId> Winners { get; private set; }

    private IEnumerable<IPlayer> NonSurrenderedPlayers => _players.Where(p => !p.HasSurrendered);

    public void Pass(PlayerId playerId)
    {
        ThrowIfGameHasEnded();
        ThrowIfPlayerIsNotOnTurn(playerId);

        _currentPlayer.IncrementConsecutivePasses();

        var allPlayersHaveReachedMaximumPasses =
            NonSurrenderedPlayers.All(p => p.ConsecutivePasses.Value >= _maximumConsecutivePasses.Value);

        if (allPlayersHaveReachedMaximumPasses)
        {
            EndGame();
        }
        else
        {
            NextTurn();
        }
    }

    public void Surrender(PlayerId playerId)
    {
        ThrowIfGameHasEnded();
        ThrowIfPlayerIsNotOnTurn(playerId);

        _currentPlayer.Surrender();

        var nonSurrenderedPlayersCount = NonSurrenderedPlayers.Count();
        ArgumentOutOfRangeException.ThrowIfZero(nonSurrenderedPlayersCount);

        if (nonSurrenderedPlayersCount == 1)
        {
            EndGame();
        }
        else
        {
            NextTurn();
        }
    }

    public void ExchangeTiles(PlayerId playerId, ImmutableArray<Tile> tilesToExchange)
    {
        ThrowIfGameHasEnded();
        ThrowIfPlayerIsNotOnTurn(playerId);

        ArgumentOutOfRangeException.ThrowIfZero(tilesToExchange.Length);
        ArgumentOutOfRangeException.ThrowIfLessThan(_bag.TilesCount, _currentPlayer.RackCapacity.Value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tilesToExchange.Length, _currentPlayer.RackCapacity.Value);

        _currentPlayer.ReturnTilesToBag(tilesToExchange, _bag);
        _bag.ShuffleTiles();

        var newTiles = _bag.DrawTiles(tilesToExchange.Length).ToImmutableArray();
        _currentPlayer.AddTilesToRack(newTiles);
        _currentPlayer.ResetConsecutivePassesCount();
        NextTurn();
    }

    private void EndGame()
    {
        var mostPlayerPoints = NonSurrenderedPlayers.Select(p => p.Points).MaxBy(p => p.Value);
        ArgumentNullException.ThrowIfNull(mostPlayerPoints);

        var winners = NonSurrenderedPlayers
            .Where(p => p.Points == mostPlayerPoints)
            .Select(p => p.Id)
            .ToImmutableArray();

        HasEnded = true;
        Winners = winners;
    }

    private void NextTurn()
    {
        var currentPlayerIndex = _players.IndexOf(_currentPlayer);
        ArgumentOutOfRangeException.ThrowIfNegative(currentPlayerIndex);

        var counter = 0;
        while (counter < _players.Count - 1)
        {
            counter++;
            currentPlayerIndex++;
            if (currentPlayerIndex == _players.Count)
            {
                currentPlayerIndex = 0;
            }

            if (_players[currentPlayerIndex].HasSurrendered)
            {
                continue;
            }

            _currentPlayer = _players[currentPlayerIndex];
            return;
        }

        throw new AllOpponentsHaveSurrenderedException(Id);
    }

    private void ThrowIfGameHasEnded()
    {
        if (HasEnded)
        {
            throw new GameHasAlreadyEndedException(Id);
        }
    }

    private void ThrowIfPlayerIsNotOnTurn(PlayerId playerId)
    {
        if (playerId != _currentPlayer.Id)
        {
            throw new PlayerIsNotOnTurnException(Id);
        }
    }

    public static Game New(
        IBag bag,
        List<IPlayer> players,
        IPlayer currentPlayer,
        MaximumConsecutivePasses maximumConsecutivePasses)
    {
        Game game = new(GameId.New(), bag, players, currentPlayer, maximumConsecutivePasses);
        return game;
    }
}
