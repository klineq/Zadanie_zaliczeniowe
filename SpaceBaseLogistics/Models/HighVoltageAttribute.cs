namespace SpaceBaseLogistics.Models;

// [WYMÓG 12: Refleksja] — atrybut odczytywany w menu diagnostycznym
[AttributeUsage(AttributeTargets.Class)]
public sealed class HighVoltageAttribute : Attribute
{
    public double MaxWatts { get; }

    public HighVoltageAttribute(double maxWatts)
    {
        MaxWatts = maxWatts;
    }
}
