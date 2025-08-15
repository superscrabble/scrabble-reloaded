namespace Gameplay.Domain.Games.Exceptions;

public sealed class GameHasAlreadyEndedException(GameId gameId) : GameException(gameId, "Game has already ended");
