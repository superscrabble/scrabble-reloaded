namespace Gameplay.Domain.Games;

public abstract class GameException(GameId gameId, string reason)
    : Exception($"An error for game with id '{gameId.Value}' has occured: {reason}.");
