namespace Gameplay.Domain.Bags;

public sealed class RandomSharedShuffler : IShuffler
{
    public void Shuffle<T>(IList<T> items)
    {
        var rng = Random.Shared;
        for (var i = items.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
