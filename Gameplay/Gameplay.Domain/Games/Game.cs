using System.Collections.Immutable;

using Gameplay.Domain.Bags;
using Gameplay.Domain.Players;
using Gameplay.Domain.Settings;
using Gameplay.Domain.Tiles;

namespace Gameplay.Domain.Games;

public sealed class Game
{
    public const int MinimumPlayersCount = 2;
    public const int MaximumPlayersCount = 4;

    private readonly IBag _bag;
    private readonly MaximumConsecutivePasses _maximumConsecutivePasses;
    private readonly ImmutableList<IPlayer> _players;

    private IPlayer _currentPlayer;

    private Game(
        GameId id,
        IBag bag,
        List<IPlayer> players,
        IPlayer currentPlayer,
        MaximumConsecutivePasses maximumConsecutivePasses,
        bool hasEnded)
    {
        Id = id;
        _bag = bag;

        var distinctPlayersCount = players.Select(p => p.Id).Distinct().Count();
        ArgumentOutOfRangeException.ThrowIfNotEqual(players.Count, distinctPlayersCount);
        ArgumentOutOfRangeException.ThrowIfLessThan(players.Count, MinimumPlayersCount);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(players.Count, MaximumPlayersCount);
        _players = [.. players];

        if (!players.Contains(currentPlayer))
        {
            throw new CurrentPlayerIsMissingException(id);
        }

        _currentPlayer = currentPlayer;
        _maximumConsecutivePasses = maximumConsecutivePasses;

        ArgumentOutOfRangeException.ThrowIfEqual(hasEnded, true);
        HasEnded = hasEnded;
    }

    private GameId Id { get; }

    public bool HasEnded { get; private set; }

    public PlayerId CurrentPlayerId => _currentPlayer.Id;

    public ImmutableArray<PlayerId> Winners { get; private set; } = [];

    private IEnumerable<IPlayer> NonSurrenderedPlayers => _players.Where(p => !p.HasSurrendered);

    public void Pass(PlayerId playerId)
    {
        ThrowIfGameHasEnded();
        ThrowIfPlayerIsNotOnTurn(playerId);

        _currentPlayer.IncrementConsecutivePasses();

        var allPlayersHaveReachedMaximumPasses =
            NonSurrenderedPlayers.All(p => p.ConsecutivePasses.Value >= _maximumConsecutivePasses.Value);

        if (!allPlayersHaveReachedMaximumPasses)
        {
            NextTurn();
            return;
        }

        SubtractRemainingTilesPointsFromPlayerScores();
        EndGame();
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

        _currentPlayer.RemoveTilesFromRack(tilesToExchange);
        _bag.AddTiles(tilesToExchange);
        _bag.ShuffleTiles();

        var newTiles = _bag.DrawTiles(tilesToExchange.Length).ToImmutableArray();
        _currentPlayer.AddTilesToRack(newTiles);
        _currentPlayer.ResetConsecutivePassesCount();

        NextTurn();
    }

    public void WriteWords(PlayerId playerId, WrittenWords writtenWords)
    {
        ThrowIfGameHasEnded();
        ThrowIfPlayerIsNotOnTurn(playerId);

        _currentPlayer.RemoveTilesFromRack(writtenWords.UsedPlayerTiles);

        var newTiles = _bag.DrawTiles(writtenWords.UsedPlayerTiles.Length).ToImmutableArray();
        _currentPlayer.AddTilesToRack(newTiles);

        _currentPlayer.IncrementPoints(writtenWords.TotalPoints);
        _currentPlayer.ResetConsecutivePassesCount();

        var gameShouldEnd = _bag.TilesCount <= 0 && !_currentPlayer.HasAnyTiles;

        if (!gameShouldEnd)
        {
            NextTurn();
            return;
        }

        AddOpponenetsRemainingTilesPointsToCurrentPlayerScore();
        SubtractRemainingTilesPointsFromPlayerScores();
        EndGame();
    }

    private void EndGame()
    {
        var mostPlayerPoints = NonSurrenderedPlayers.Max(p => p.Score);

        var winners = NonSurrenderedPlayers
            .Where(p => p.Score == mostPlayerPoints)
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

    private void AddOpponenetsRemainingTilesPointsToCurrentPlayerScore()
    {
        var opponentsPoints = NonSurrenderedPlayers
            .Where(p => p.Id != _currentPlayer.Id)
            .Sum(p => p.RemainingTilesPoints);

        _currentPlayer.IncrementPoints(opponentsPoints);
    }

    private void SubtractRemainingTilesPointsFromPlayerScores()
    {
        foreach (var player in NonSurrenderedPlayers)
        {
            player.SubtractRemainingTilesPointsFromScore();
        }
    }

    public static Game Create(
        GameId id,
        IBag bag,
        List<IPlayer> players,
        IPlayer currentPlayer,
        MaximumConsecutivePasses maximumConsecutivePasses,
        bool hasEnded)
    {
        var game = new Game(
            id,
            bag,
            players,
            currentPlayer,
            maximumConsecutivePasses,
            hasEnded);

        return game;
    }
}
