using System.Collections.Immutable;

using Gameplay.Domain.Bags;
using Gameplay.Domain.Boards;
using Gameplay.Domain.Games;
using Gameplay.Domain.Games.Exceptions;
using Gameplay.Domain.Players;
using Gameplay.Domain.Tiles;
using Gameplay.Domain.Words;

using NSubstitute;

using ReflectionMagic;

using Shouldly;

namespace Gameplay.Domain.UnitTests;

public sealed class GameTests
{
    [Fact]
    public void Pass_ShouldThrowException_WhenGameHasEnded()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MaximumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        game.AsDynamic().HasEnded = true;

        //Act
        var act = () => game.Pass(currentPlayer.Id);

        //Assert
        act.ShouldThrow<GameHasAlreadyEndedException>();
    }

    [Fact]
    public void Pass_ShouldThrowException_WhenPlayerIsNotOnTurn()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MaximumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        var act = () => game.Pass(players.Last().Id);

        //Assert
        act.ShouldThrow<PlayerIsNotOnTurnException>();
    }

    [Fact]
    public void Pass_ShouldMoveToNextTurn_WhenPlayersHaveNotYetReachedMaximumPasses()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var player1 = Substitute.For<IPlayer>();
        var threePasses = ConsecutivePasses.Initial.Increment().Increment().Increment();
        player1.Id.Returns(PlayerId.New());
        player1.ConsecutivePasses.Returns(threePasses);
        player1.HasSurrendered.Returns(true);

        var player2 = Substitute.For<IPlayer>();
        var onePass = ConsecutivePasses.Initial.Increment();
        player2.Id.Returns(PlayerId.New());
        player2.ConsecutivePasses.Returns(onePass);
        player2.HasSurrendered.Returns(false);

        var player3 = Substitute.For<IPlayer>();
        player3.Id.Returns(PlayerId.New());
        player3.HasSurrendered.Returns(true);

        var player4 = Substitute.For<IPlayer>();
        player4.Id.Returns(PlayerId.New());
        var twoPasses = ConsecutivePasses.Initial.Increment().Increment();
        player4.ConsecutivePasses.Returns(twoPasses);
        player4.HasSurrendered.Returns(false);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            [player1, player2, player3, player4],
            player4,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        game.Pass(player4.Id);

        //Assert
        player4.Received(1).IncrementConsecutivePasses();
        game.CurrentPlayerId.ShouldBeEquivalentTo(player2.Id);
        game.HasEnded.ShouldBeFalse();
        game.Winners.ShouldBeEmpty();
    }

    [Fact]
    public void
        Pass_ShouldSubtractRemainingTilesPointsFromPlayersScoresAndEndGame_WhenAllPlayersHaveReachedMaximumPasses()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var player1 = Substitute.For<IPlayer>();
        var threePasses = ConsecutivePasses.Initial.Increment().Increment().Increment();
        player1.Id.Returns(PlayerId.New());
        player1.ConsecutivePasses.Returns(threePasses);
        player1.HasSurrendered.Returns(false);
        player1.Score.Returns(78);

        var player2 = Substitute.For<IPlayer>();
        var twoPasses = ConsecutivePasses.Initial.Increment().Increment();
        player2.Id.Returns(PlayerId.New());
        player2.ConsecutivePasses.Returns(twoPasses);
        player2.HasSurrendered.Returns(true);
        player2.Score.Returns(221);

        var player3 = Substitute.For<IPlayer>();
        var twoPassesAgain = ConsecutivePasses.Initial.Increment().Increment();
        player3.Id.Returns(PlayerId.New());
        player3.ConsecutivePasses.Returns(twoPassesAgain);
        player3.HasSurrendered.Returns(false);
        player3.Score.Returns(71);

        var player4 = Substitute.For<IPlayer>();
        player4.Id.Returns(PlayerId.New());
        var twoPassesAlso = ConsecutivePasses.Initial.Increment().Increment();
        player4.ConsecutivePasses.Returns(twoPassesAlso);
        player4.HasSurrendered.Returns(false);
        player4.Score.Returns(78);

        var players = new List<IPlayer> { player1, player2, player3, player4 };

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            player4,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        game.Pass(player4.Id);

        //Assert
        player4.Received(1).IncrementConsecutivePasses();
        game.CurrentPlayerId.ShouldBeEquivalentTo(player4.Id);

        var nonSurrenderedPlayers = new List<IPlayer> { player1, player3, player4 };
        nonSurrenderedPlayers.ForEach(p => p.Received(1).SubtractRemainingTilesPointsFromScore());

        game.HasEnded.ShouldBeTrue();
        game.Winners.ShouldContain(player1.Id);
        game.Winners.ShouldContain(player4.Id);
        game.Winners.Length.ShouldBe(2);
    }

    [Fact]
    public void Surrender_ShouldThrowException_WhenGameHasEnded()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MaximumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        game.AsDynamic().HasEnded = true;

        //Act
        var act = () => game.Surrender(currentPlayer.Id);

        //Assert
        act.ShouldThrow<GameHasAlreadyEndedException>();
    }

    [Fact]
    public void Surrender_ShouldThrowException_WhenPlayerIsNotOnTurn()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MaximumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        var act = () => game.Surrender(players.Last().Id);

        //Assert
        act.ShouldThrow<PlayerIsNotOnTurnException>();
    }

    [Fact]
    public void Surrender_ShouldThrowException_WhenAllPlayersHaveSurrender()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var player1 = Substitute.For<IPlayer>();
        player1.Id.Returns(PlayerId.New());
        player1.HasSurrendered.Returns(true);

        var player2 = Substitute.For<IPlayer>();
        player2.Id.Returns(PlayerId.New());
        player2.HasSurrendered.Returns(true);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            [player1, player2],
            player1,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        var act = () => game.Surrender(player1.Id);

        //Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
        player1.Received(1).Surrender();
    }

    [Fact]
    public void Surrender_ShouldEndGame_WhenOnlyOnePlayerIsLeft()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var player1 = Substitute.For<IPlayer>();
        player1.Id.Returns(PlayerId.New());
        player1.HasSurrendered.Returns(false);
        player1.Score.Returns(8);

        var player2 = Substitute.For<IPlayer>();
        player2.Id.Returns(PlayerId.New());
        player2.HasSurrendered.Returns(true);
        player2.Score.Returns(21);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            [player1, player2],
            player1,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        game.Surrender(player1.Id);

        //Assert
        player1.Received(1).Surrender();
        game.HasEnded.ShouldBeTrue();
        game.Winners.ShouldContain(player1.Id);
        game.Winners.ShouldHaveSingleItem();
    }

    [Fact]
    public void Surrender_ShouldMoveToNextTurn_WhenThereAreEnoughPlayersLeft()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var player1 = Substitute.For<IPlayer>();
        player1.Id.Returns(PlayerId.New());
        player1.HasSurrendered.Returns(false);
        player1.Score.Returns(8);

        var player2 = Substitute.For<IPlayer>();
        player2.Id.Returns(PlayerId.New());
        player2.HasSurrendered.Returns(true);
        player2.Score.Returns(21);

        var player3 = Substitute.For<IPlayer>();
        player3.Id.Returns(PlayerId.New());
        player3.HasSurrendered.Returns(false);
        player3.Score.Returns(123);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            [player1, player2, player3],
            player1,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        game.Surrender(player1.Id);

        //Assert
        player1.Received(1).Surrender();
        game.HasEnded.ShouldBeFalse();
        game.Winners.ShouldBeEmpty();
        game.CurrentPlayerId.ShouldBe(player3.Id);
    }

    [Fact]
    public void ExchangeTiles_ShouldThrowException_WhenGameHasEnded()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        game.AsDynamic().HasEnded = true;

        //Act
        var act = () => game.ExchangeTiles(currentPlayer.Id, []);

        //Assert
        act.ShouldThrow<GameHasAlreadyEndedException>();
    }

    [Fact]
    public void ExchangeTiles_ShouldThrowException_WhenPlayerIsNotOnTurn()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        var act = () => game.ExchangeTiles(players.Last().Id, []);

        //Assert
        act.ShouldThrow<PlayerIsNotOnTurnException>();
    }

    [Fact]
    public void ExchangeTiles_ShouldThrowException_WhenTheNumberOfTilesToExchangeIsZero()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        var act = () => game.ExchangeTiles(currentPlayer.Id, []);

        //Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ExchangeTiles_ShouldThrowException_WhenTilesInBagAreLessThanPlayerRackCapacity()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        bag.TilesCount.Returns(6);

        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();
        var rackCapacity = RackCapacity.From(7);
        currentPlayer.RackCapacity.Returns(rackCapacity);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        var tilesToExchange = ImmutableArray.Create((Tile)null!);

        //Act
        var act = () => game.ExchangeTiles(currentPlayer.Id, tilesToExchange);

        //Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ExchangeTiles_ShouldThrowException_WhenTilesToExchangeAreGreaterThanCurrentPlayerRackCapacity()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        bag.TilesCount.Returns(7);

        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();
        var rackCapacity = RackCapacity.From(7);
        currentPlayer.RackCapacity.Returns(rackCapacity);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        var tilesToExchange = Enumerable
            .Range(1, rackCapacity.Value + 1)
            .Select(Tile (_) => null!)
            .ToImmutableArray();

        //Act
        var act = () => game.ExchangeTiles(currentPlayer.Id, tilesToExchange);

        //Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ExchangeTiles_ShouldSwapTilesAndMoveToNextTurn_WhenTilesToExchangeAreCorrect()
    {
        //Arrange
        var rackCapacity = RackCapacity.From(7);

        var tilesToExchange = Enumerable
            .Range(1, rackCapacity.Value)
            .Select(Tile (_) => null!)
            .ToImmutableArray();

        var bag = Substitute.For<IBag>();
        var drawnTilesFromBag = Enumerable
            .Range(1, rackCapacity.Value)
            .Select(Tile (_) => null!)
            .ToImmutableArray();

        bag.DrawTiles(tilesToExchange.Length).Returns(drawnTilesFromBag);
        bag.TilesCount.Returns(7);

        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();
        currentPlayer.RackCapacity.Returns(rackCapacity);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        //Act
        game.ExchangeTiles(currentPlayer.Id, tilesToExchange);

        //Assert
        currentPlayer.Received(1).RemoveTilesFromRack(tilesToExchange);
        bag.Received(1).AddTiles(tilesToExchange);
        bag.Received(1).ShuffleTiles();

        bag.Received(1).DrawTiles(tilesToExchange.Length);
        currentPlayer.Received(1).AddTilesToRack(drawnTilesFromBag);
        currentPlayer.Received(1).ResetConsecutivePassesCount();

        game.CurrentPlayerId.ShouldNotBe(currentPlayer.Id);
        game.CurrentPlayerId.ShouldBe(players.Last().Id);
    }

    [Fact]
    public void WriteWords_ShouldThrowException_WhenGameHasEnded()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        game.AsDynamic().HasEnded = true;

        var validWords = Substitute.For<IValidWords>();

        //Act
        var act = () => game.WriteWords(currentPlayer.Id, validWords);

        //Assert
        act.ShouldThrow<GameHasAlreadyEndedException>();
    }

    [Fact]
    public void WriteWords_ShouldThrowException_WhenPlayerIsNotOnTurn()
    {
        //Arrange
        var bag = Substitute.For<IBag>();
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount).Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            false);

        var validWords = Substitute.For<IValidWords>();

        //Act
        var act = () => game.WriteWords(players.Last().Id, validWords);

        //Assert
        act.ShouldThrow<PlayerIsNotOnTurnException>();
    }

    [Fact]
    public void WriteWords_ShouldPerformAllMandatoryPlayerActionsCorrectly_WhenGameHasNotYetEndedAndPlayerIsOnTurn()
    {
        //Arrange
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount)
            .Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();

        var validWords = Substitute.For<IValidWords>();

        var tiles = new List<Tile>
            {
                new(TileType.Normal, 1, 'A'),
                new(TileType.Normal, 3, 'C'),
                new(TileType.Normal, 2, 'B'),
                new(TileType.Normal, 5, 'P')
            }
            .ToImmutableArray();

        validWords.Tiles.Returns(tiles);

        var tilePositions = tiles
            .Select((t, i) => new KeyValuePair<Tile, BoardCellPosition>(t, new BoardCellPosition(2, i + 1)))
            .ToImmutableDictionary();

        validWords.TilePositions.Returns(tilePositions);

        var totalPoints = tiles.Sum(t => t.Points);
        validWords.TotalPoints.Returns(totalPoints);

        var bag = Substitute.For<IBag>();
        var tilesInBag = Enumerable.Range(1, 3)
            .Select(_ => new Tile(TileType.Normal, 2, 'B'))
            .ToImmutableArray();

        bag.DrawTiles(tiles.Length).Returns(tilesInBag);
        bag.TilesCount.Returns(5);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(1),
            false);

        //Act
        game.WriteWords(currentPlayer.Id, validWords);

        //Assert
        currentPlayer.Received(1).RemoveTilesFromRack(tiles);
        _ = validWords.Received(2).Tiles;

        bag.Received(1).DrawTiles(validWords.Tiles.Length);
        currentPlayer.Received(1).AddTilesToRack(tilesInBag);

        currentPlayer.Received(1).IncrementPoints(totalPoints);
        _ = validWords.Received(1).TotalPoints;
        currentPlayer.ResetConsecutivePassesCount();

        _ = validWords.Received(1).TilePositions;

        foreach (var tilePosition in validWords.TilePositions)
        {
            board.Received(1).SetTile(tilePosition.Key, tilePosition.Value);
        }

        _ = bag.Received(1).TilesCount;
    }

    [Fact]
    public void WriteWords_ShouldMoveToNextTurn_WhenBagStillHasRemainingTiles()
    {
        //Arrange
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount)
            .Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();
        currentPlayer.HasAnyTiles.Returns(false);

        var validWords = Substitute.For<IValidWords>();

        var tiles = new List<Tile>
            {
                new(TileType.Normal, 1, 'A'),
                new(TileType.Normal, 3, 'C'),
                new(TileType.Normal, 2, 'B'),
                new(TileType.Normal, 5, 'P')
            }
            .ToImmutableArray();

        validWords.Tiles.Returns(tiles);

        var tilePositions = tiles
            .Select((t, i) => new KeyValuePair<Tile, BoardCellPosition>(t, new BoardCellPosition(2, i + 1)))
            .ToImmutableDictionary();

        validWords.TilePositions.Returns(tilePositions);

        var totalPoints = tiles.Sum(t => t.Points);
        validWords.TotalPoints.Returns(totalPoints);

        var bag = Substitute.For<IBag>();
        var tilesInBag = Enumerable.Range(1, 3)
            .Select(_ => new Tile(TileType.Normal, 2, 'B'))
            .ToImmutableArray();

        bag.DrawTiles(tiles.Length).Returns(tilesInBag);
        bag.TilesCount.Returns(12);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(1),
            false);

        //Act
        game.WriteWords(currentPlayer.Id, validWords);

        //Assert
        game.CurrentPlayerId.ShouldBe(players.Last().Id);
    }

    [Fact]
    public void WriteWords_ShouldMoveToNextTurn_WhenBagIsEmptyButPlayerStillHasTiles()
    {
        //Arrange
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount)
            .Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                return player;
            })
            .ToList();

        var currentPlayer = players.First();
        currentPlayer.HasAnyTiles.Returns(true);

        var validWords = Substitute.For<IValidWords>();

        var tiles = new List<Tile>
            {
                new(TileType.Normal, 1, 'A'),
                new(TileType.Normal, 3, 'C'),
                new(TileType.Normal, 2, 'B'),
                new(TileType.Normal, 5, 'P')
            }
            .ToImmutableArray();

        validWords.Tiles.Returns(tiles);

        var tilePositions = tiles
            .Select((t, i) => new KeyValuePair<Tile, BoardCellPosition>(t, new BoardCellPosition(2, i + 1)))
            .ToImmutableDictionary();

        validWords.TilePositions.Returns(tilePositions);

        var totalPoints = tiles.Sum(t => t.Points);
        validWords.TotalPoints.Returns(totalPoints);

        var bag = Substitute.For<IBag>();
        var tilesInBag = Enumerable.Range(1, 3)
            .Select(_ => new Tile(TileType.Normal, 2, 'B'))
            .ToImmutableArray();

        bag.DrawTiles(tiles.Length).Returns(tilesInBag);
        bag.TilesCount.Returns(0);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(1),
            false);

        //Act
        game.WriteWords(currentPlayer.Id, validWords);

        //Assert
        game.CurrentPlayerId.ShouldBe(players.Last().Id);
    }

    [Fact]
    public void WriteWords_ShouldCalculateFinalPlayerScoresAndEndGame_WhenBagIsEmptyAndPlayerHasNoTiles()
    {
        //Arrange
        var board = Substitute.For<IBoard>();

        var players = Enumerable.Range(1, Game.MinimumPlayersCount)
            .Select(_ =>
            {
                var player = Substitute.For<IPlayer>();
                player.Id.Returns(PlayerId.New());
                player.RemainingTilesPoints.Returns(33);
                return player;
            })
            .ToList();

        var currentPlayer = players.First();
        currentPlayer.HasAnyTiles.Returns(false);
        currentPlayer.Score.Returns(20);

        var validWords = Substitute.For<IValidWords>();

        var tiles = new List<Tile>
            {
                new(TileType.Normal, 1, 'A'),
                new(TileType.Normal, 3, 'C'),
                new(TileType.Normal, 2, 'B'),
                new(TileType.Normal, 5, 'P')
            }
            .ToImmutableArray();

        validWords.Tiles.Returns(tiles);

        var tilePositions = tiles
            .Select((t, i) => new KeyValuePair<Tile, BoardCellPosition>(t, new BoardCellPosition(2, i + 1)))
            .ToImmutableDictionary();

        validWords.TilePositions.Returns(tilePositions);

        var totalPoints = tiles.Sum(t => t.Points);
        validWords.TotalPoints.Returns(totalPoints);

        var bag = Substitute.For<IBag>();
        var tilesInBag = Enumerable.Range(1, 3)
            .Select(_ => new Tile(TileType.Normal, 2, 'B'))
            .ToImmutableArray();

        bag.DrawTiles(tiles.Length).Returns(tilesInBag);
        bag.TilesCount.Returns(0);

        var game = Game.Create(
            GameId.New(),
            bag,
            board,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(1),
            false);

        //Act
        game.WriteWords(currentPlayer.Id, validWords);

        //Assert
        currentPlayer.Received(1).IncrementPoints(players.Last().RemainingTilesPoints);

        foreach (var player in players)
        {
            player.Received(1).SubtractRemainingTilesPointsFromScore();
        }

        game.CurrentPlayerId.ShouldBe(currentPlayer.Id);
        game.HasEnded.ShouldBeTrue();
        game.Winners.Length.ShouldBe(1);
        game.Winners.ShouldContain(currentPlayer.Id);
    }
}
