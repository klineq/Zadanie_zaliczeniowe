namespace SpaceBaseLogistics.Services;

public sealed class SpaceBaseState
{
  public string BaseName { get; set; } = "Orbital Hub Alpha";
  public double StoredEnergyKwh { get; set; }
  public Dictionary<string, double> Inventory { get; set; } = new();
  public List<MachineStateDto> Machines { get; set; } = [];
}

public sealed class MachineStateDto
{
  public string Type { get; set; } = "";
  public string Name { get; set; } = "";
  public double CurrentHeat { get; set; }
  public bool IsRunning { get; set; }
}
