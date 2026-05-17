namespace SpaceBaseLogistics.Models;

public static class KnownResources
{
  public static IReadOnlyList<Resource> All { get; } =
  [
      Miner.IronOre,
      Smelter.IronPlate
  ];
}
