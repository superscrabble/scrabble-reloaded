namespace Gameplay.Domain.Players;

public sealed class PlayerHasSurrenderedException(PlayerId id) : PlayerException(id, "Player has surrendered");
