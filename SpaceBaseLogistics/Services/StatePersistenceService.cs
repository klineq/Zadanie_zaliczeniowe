using System.Text.Json;
using SpaceBaseLogistics.Core;
using SpaceBaseLogistics.Models;

namespace SpaceBaseLogistics.Services;

// [WYMÓG 1: Klasy]
public sealed class StatePersistenceService
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  private readonly string _filePath;

  public StatePersistenceService(string filePath) => _filePath = filePath;

  // [WYMÓG 11: Programowanie asynchroniczne]
  public async Task SaveAsync(SpaceBaseSimulator simulator, CancellationToken cancellationToken = default)
  {
    var state = new SpaceBaseState
    {
      BaseName = simulator.BaseName,
      StoredEnergyKwh = simulator.StoredEnergyKwh,
      // [WYMÓG 3: Właściwości / Indeksatory] — magazyn serializowany przez indeksator
      Inventory = new Dictionary<string, double>(simulator.Inventory.Snapshot),
      Machines = simulator.Machines.Select(m => new MachineStateDto
      {
        Type = m.GetType().Name,
        Name = m.Name,
        CurrentHeat = m.CurrentHeat,
        IsRunning = m.IsRunning
      }).ToList()
    };

    await using FileStream stream = File.Create(_filePath);
    await JsonSerializer.SerializeAsync(stream, state, JsonOptions, cancellationToken);
  }

  // [WYMÓG 11: Programowanie asynchroniczne]
  public async Task<SpaceBaseState?> LoadAsync(CancellationToken cancellationToken = default)
  {
    if (!File.Exists(_filePath))
    {
      return null;
    }

    await using FileStream stream = File.OpenRead(_filePath);
    return await JsonSerializer.DeserializeAsync<SpaceBaseState>(stream, JsonOptions, cancellationToken);
  }

  public void ApplyLoadedState(SpaceBaseSimulator simulator, SpaceBaseState state) =>
      simulator.RestoreFromSave(state);
}
