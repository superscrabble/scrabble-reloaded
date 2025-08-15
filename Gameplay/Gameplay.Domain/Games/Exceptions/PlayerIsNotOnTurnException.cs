namespace Gameplay.Domain.Games.Exceptions;

public sealed class PlayerIsNotOnTurnException(GameId gameId) : GameException(gameId, "Player is not on turn");
