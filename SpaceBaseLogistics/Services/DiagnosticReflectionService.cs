using System.Reflection;
using SpaceBaseLogistics.Models;

namespace SpaceBaseLogistics.Services;

// [WYMÓG 1: Klasy]
public sealed class DiagnosticReflectionService
{
  // [WYMÓG 12: Refleksja]
  public IReadOnlyList<string> InspectMachine(Machine machine)
  {
    var lines = new List<string>();
    Type type = machine.GetType();

    lines.Add($"Typ: {type.FullName}");

  // [WYMÓG 12: Refleksja] — odczyt customowego atrybutu
    HighVoltageAttribute? hv = type.GetCustomAttribute<HighVoltageAttribute>();
    if (hv is not null)
    {
      lines.Add($"[HighVoltage] limit: {hv.MaxWatts} W");
    }

    PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    foreach (PropertyInfo property in properties)
    {
      object? value = property.GetValue(machine);
      lines.Add($"{property.Name} = {value}");
    }

    return lines;
  }
}
