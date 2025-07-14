namespace Gameplay.Domain.Games;

public sealed class PlayerIsNotOnTurnException(GameId gameId) : GameException(gameId, "Player is not on turn");
