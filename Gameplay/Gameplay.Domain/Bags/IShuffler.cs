namespace Gameplay.Domain.Bags;

public interface IShuffler
{
    void Shuffle<T>(IList<T> items);
}
