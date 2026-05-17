using SpaceBaseLogistics.Models;

namespace SpaceBaseLogistics.Core;

// [WYMÓG 1: Klasy]
public class Inventory
{
    // [WYMÓG 8: Typy ogólne / Kolekcje]
    private readonly Dictionary<string, double> _amounts = new(StringComparer.OrdinalIgnoreCase);

    public void Add(Resource resource, double amount)
    {
        if (!_amounts.ContainsKey(resource.Name))
        {
            _amounts[resource.Name] = 0;
        }

        _amounts[resource.Name] += amount;
    }

    public bool TryTake(string resourceName, double amount)
    {
        if (!TryGetAmount(resourceName, out double current) || current < amount)
        {
            return false;
        }

        _amounts[resourceName] = current - amount;
        return true;
    }

    public bool TryGetAmount(string resourceName, out double amount) =>
        _amounts.TryGetValue(resourceName, out amount);

    // [WYMÓG 3: Właściwości / Indeksatory]
    public double this[string resourceName]
    {
        get => _amounts.TryGetValue(resourceName, out double value) ? value : 0;
        set => _amounts[resourceName] = value;
    }

    public IReadOnlyDictionary<string, double> Snapshot =>
        new Dictionary<string, double>(_amounts);

    public void Clear() => _amounts.Clear();
}
