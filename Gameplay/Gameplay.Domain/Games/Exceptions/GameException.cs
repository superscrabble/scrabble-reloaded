namespace Gameplay.Domain.Games.Exceptions;

public abstract class GameException(GameId gameId, string reason)
    : Exception($"An error for game with id '{gameId.Value}' has occured: {reason}.");
