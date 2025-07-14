namespace Gameplay.Domain.Players;

public sealed class PlayerDoesNotOwnTileException(PlayerId id) : PlayerException(id, "Player does not own tile");
