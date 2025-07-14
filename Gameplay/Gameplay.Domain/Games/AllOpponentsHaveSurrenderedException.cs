namespace Gameplay.Domain.Games;

public sealed class AllOpponentsHaveSurrenderedException(GameId gameId)
    : GameException(gameId, "All opponents have surrendered");
