namespace SpaceBaseLogistics.Core;

// [WYMÓG 8: Typy ogólne / Kolekcje] — własna klasa generyczna
public class ProductionQueue<T>
{
    // [WYMÓG 8: Typy ogólne / Kolekcje]
    private readonly Queue<T> _items = new();

    public int Count => _items.Count;

    public void Enqueue(T item) => _items.Enqueue(item);

    public bool TryDequeue(out T? item)
    {
        if (_items.Count == 0)
        {
            item = default;
            return false;
        }

        item = _items.Dequeue();
        return true;
    }

    public IReadOnlyList<T> PeekAll() => _items.ToList();
}
