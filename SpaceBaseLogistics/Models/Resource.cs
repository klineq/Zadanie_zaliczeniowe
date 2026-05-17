namespace SpaceBaseLogistics.Models;

// [WYMÓG 1: Klasy]
public class Resource
{
    public string Name { get; }
    public string Unit { get; }

    public Resource(string name, string unit)
    {
        Name = name;
        Unit = unit;
    }

    public override string ToString() => $"{Name} ({Unit})";
}
