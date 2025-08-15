namespace Gameplay.Domain.Games.Exceptions;

public sealed class AllOpponentsHaveSurrenderedException(GameId gameId)
    : GameException(gameId, "All opponents have surrendered");
