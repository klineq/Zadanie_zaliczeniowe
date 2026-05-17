using SpaceBaseLogistics.Core;

namespace SpaceBaseLogistics.Models;

// [WYMÓG 5: Dziedziczenie]
[HighVoltage(450)]
public class Miner : Machine
{
  public static readonly Resource IronOre = new("IronOre", "kg");

  // [WYMÓG 2: Konstruktory]
  public Miner(string name)
      : base(name, powerDrawKwh: 12, heatLimit: 80)
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

    inventory.Add(IronOre, 5);
  }
}
