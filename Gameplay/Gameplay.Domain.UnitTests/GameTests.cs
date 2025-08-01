using Gameplay.Domain.Bags;
using Gameplay.Domain.Games;
using Gameplay.Domain.Players;
using Gameplay.Domain.Settings;

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

        var players = Enumerable.Range(1, Game.MaximumPlayersCount).Select(x =>
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
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        game.AsDynamic().HasEnded = true;

        //Act
        Action act = () => game.Pass(currentPlayer.Id);

        //Assert
        act.ShouldThrow<GameHasAlreadyEndedException>();
    }

    [Fact]
    public void Pass_ShouldThrowException_WhenPlayerIsNotOnTurn()
    {
        //Arrange
        var bag = Substitute.For<IBag>();

        var players = Enumerable.Range(1, Game.MaximumPlayersCount).Select(x =>
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
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        //Act
        Action act = () => game.Pass(players.Last().Id);

        //Assert
        act.ShouldThrow<PlayerIsNotOnTurnException>();
    }

    [Fact]
    public void Pass_ShouldMoveToNextTurn_WhenPlayersHaveNotYetReachedMaximumPasses()
    {
        //Arrange
        var bag = Substitute.For<IBag>();

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

        var currentPlayer = player4;

        var game = Game.Create(
            GameId.New(),
            bag,
            [player1, player2, player3, player4],
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        //Act
        game.Pass(currentPlayer.Id);

        //Assert
        currentPlayer.Received().IncrementConsecutivePasses();
        game.CurrentPlayerId.ShouldBeEquivalentTo(player2.Id);
        game.HasEnded.ShouldBeFalse();
        game.Winners.ShouldBeEmpty();
    }

    [Fact]
    public void Pass_ShouldSubtractRemainingTilesPointsFromPlayersScoresAndEndGame_WhenAllPlayersHaveReachedMaximumPasses()
    {
        //Arrange
        var bag = Substitute.For<IBag>();

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

        var currentPlayer = player4;

        var players = new List<IPlayer> { player1, player2, player3, player4 };

        var game = Game.Create(
            GameId.New(),
            bag,
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        //Act
        game.Pass(currentPlayer.Id);

        //Assert
        currentPlayer.Received().IncrementConsecutivePasses();
        game.CurrentPlayerId.ShouldBeEquivalentTo(currentPlayer.Id);

        var nonSurrenderedPlayers = new List<IPlayer> { player1, player3, player4 };
        nonSurrenderedPlayers.ForEach(p => p.Received().SubtractRemainingTilesPointsFromScore());

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

        var players = Enumerable.Range(1, Game.MaximumPlayersCount).Select(x =>
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
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        game.AsDynamic().HasEnded = true;

        //Act
        Action act = () => game.Surrender(currentPlayer.Id);

        //Assert
        act.ShouldThrow<GameHasAlreadyEndedException>();
    }

    [Fact]
    public void Surrender_ShouldThrowException_WhenPlayerIsNotOnTurn()
    {
        //Arrange
        var bag = Substitute.For<IBag>();

        var players = Enumerable.Range(1, Game.MaximumPlayersCount).Select(x =>
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
            players,
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        //Act
        Action act = () => game.Surrender(players.Last().Id);

        //Assert
        act.ShouldThrow<PlayerIsNotOnTurnException>();
    }

    [Fact]
    public void Surrender_ShouldThrowException_WhenAllPlayersHaveSurrender()
    {
        //Arrange
        var bag = Substitute.For<IBag>();

        var player1 = Substitute.For<IPlayer>();
        player1.Id.Returns(PlayerId.New());
        player1.HasSurrendered.Returns(true);

        var player2 = Substitute.For<IPlayer>();
        player2.Id.Returns(PlayerId.New());
        player2.HasSurrendered.Returns(true);

        var currentPlayer = player1;

        var game = Game.Create(
            GameId.New(),
            bag,
            [player1, player2],
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        //Act
        Action act = () => game.Surrender(currentPlayer.Id);

        //Assert
        act.ShouldThrow<ArgumentOutOfRangeException>();
        currentPlayer.Received().Surrender();
    }

    [Fact]
    public void Surrender_ShouldEndGame_WhenOnlyOnePlayerIsLeft()
    {
        //Arrange
        var bag = Substitute.For<IBag>();

        var player1 = Substitute.For<IPlayer>();
        player1.Id.Returns(PlayerId.New());
        player1.HasSurrendered.Returns(false);
        player1.Score.Returns(8);

        var player2 = Substitute.For<IPlayer>();
        player2.Id.Returns(PlayerId.New());
        player2.HasSurrendered.Returns(true);
        player2.Score.Returns(21);

        var currentPlayer = player1;

        var game = Game.Create(
            GameId.New(),
            bag,
            [player1, player2],
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        //Act
        game.Surrender(currentPlayer.Id);

        //Assert
        currentPlayer.Received().Surrender();
        game.HasEnded.ShouldBeTrue();
        game.Winners.ShouldContain(currentPlayer.Id);
        game.Winners.ShouldHaveSingleItem();
    }

    [Fact]
    public void Surrender_ShouldMoveToNextTurn_WhenThereAreEnoughPlayersLeft()
    {
        //Arrange
        var bag = Substitute.For<IBag>();

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

        var currentPlayer = player1;

        var game = Game.Create(
            GameId.New(),
            bag,
            [player1, player2, player3],
            currentPlayer,
            MaximumConsecutivePasses.From(2),
            hasEnded: false);

        //Act
        game.Surrender(currentPlayer.Id);

        //Assert
        currentPlayer.Received().Surrender();
        game.HasEnded.ShouldBeFalse();
        game.Winners.ShouldBeEmpty();
        game.CurrentPlayerId.ShouldBe(player3.Id);
    }
}
