namespace Gameplay.Domain.Games;

public sealed class GameHasAlreadyEndedException(GameId gameId) : GameException(gameId, "Game has already ended");
