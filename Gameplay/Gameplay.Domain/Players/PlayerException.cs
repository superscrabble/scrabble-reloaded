namespace Gameplay.Domain.Players;

public abstract class PlayerException(PlayerId playerId, string reason)
    : Exception($"An error for player with id '{playerId.Value}' has occured: {reason}.");
