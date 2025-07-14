namespace Gameplay.Domain.Games;

public sealed class CurrentPlayerIsMissingException(GameId gameId)
    : GameException(gameId, "Current player is missing from the list of players");
