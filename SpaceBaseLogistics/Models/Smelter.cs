using SpaceBaseLogistics.Core;

namespace SpaceBaseLogistics.Models;

// [WYMÓG 5: Dziedziczenie]
[HighVoltage(600)]
public class Smelter : Machine
{
  public static readonly Resource IronPlate = new("IronPlate", "szt.");

  private const double OrePerTick = 3;
  private const double PlatesPerTick = 2;

  // [WYMÓG 2: Konstruktory]
  public Smelter(string name)
      : base(name, powerDrawKwh: 18, heatLimit: 95)
  {
  }

  // [WYMÓG 6: Polimorfizm]
  public override void ProcessTick(Inventory inventory)
  {
    base.ProcessTick(inventory);

    if (!IsRunning)
    {
      return;
    }

    if (!inventory.TryTake(Miner.IronOre.Name, OrePerTick))
    {
      return;
    }

    inventory.Add(IronPlate, PlatesPerTick);
  }
}
