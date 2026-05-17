using SpaceBaseLogistics.Core;
using SpaceBaseLogistics.Models;

namespace SpaceBaseLogistics.Services;

// [WYMÓG 1: Klasy]
public sealed class SpaceBaseSimulator
{
  // [WYMÓG 8: Typy ogólne / Kolekcje]
  private readonly List<Machine> _machines = [];
  private readonly Inventory _inventory = new();
  private readonly ProductionQueue<string> _logQueue = new();

  public string BaseName { get; set; } = "Orbital Hub Alpha";

  public double StoredEnergyKwh { get; private set; } = 500;

  public IReadOnlyList<Machine> Machines => _machines;

  public Inventory Inventory => _inventory;

  public ProductionQueue<string> LogQueue => _logQueue;

  public event EventHandler<string>? AlertRaised;

  public SpaceBaseSimulator()
  {
    WireMachineEvents();
  }

  public void AddMachine(Machine machine)
  {
    machine.OnOverheat += HandleMachineOverheat;
    _machines.Add(machine);
    _logQueue.Enqueue($"Dodano maszynę: {machine.Name}");
  }

  public void RunTick()
  {
    double energyBeforeTick = BaseEnergyTracker.TotalConsumedKwh;

    // [WYMÓG 6: Polimorfizm] — wywołanie wirtualnego ProcessTick()
    foreach (Machine machine in _machines)
    {
      machine.ProcessTick(_inventory);
    }

    double tickConsumption = BaseEnergyTracker.TotalConsumedKwh - energyBeforeTick;
    StoredEnergyKwh = Math.Max(0, StoredEnergyKwh - tickConsumption * 0.05);
    _logQueue.Enqueue($"Cykl zakończony. Zużycie w cyklu: {tickConsumption:F2} kWh.");
  }

  public void RestoreFromSave(SpaceBaseState state)
  {
    ClearMachines();
    BaseName = state.BaseName;
    StoredEnergyKwh = state.StoredEnergyKwh;
    Inventory.Clear();

    foreach (KeyValuePair<string, double> entry in state.Inventory)
    {
      Inventory[entry.Key] = entry.Value;
    }

    foreach (MachineStateDto dto in state.Machines)
    {
      Machine machine = dto.Type switch
      {
        nameof(Miner) => new Miner(dto.Name),
        nameof(Smelter) => new Smelter(dto.Name),
        _ => throw new InvalidOperationException($"Nieznany typ maszyny: {dto.Type}")
      };

      machine.ApplySavedState(dto.CurrentHeat, dto.IsRunning);
      AddMachine(machine);
    }

    _logQueue.Enqueue("Wczytano stan bazy z pliku JSON.");
  }

  public void ClearMachines()
  {
    foreach (Machine machine in _machines)
    {
      machine.OnOverheat -= HandleMachineOverheat;
    }

    _machines.Clear();
  }

  public void TriggerDemoOverheat()
  {
    Machine? target = _machines.FirstOrDefault(m => m.IsRunning);
    if (target is null)
    {
      AlertRaised?.Invoke(this, "Brak aktywnej maszyny do awarii.");
      return;
    }

    target.ForceOverheatForDemo();
  }

  public void ConnectMachines(int firstIndex, int secondIndex)
  {
    if (firstIndex < 0 || firstIndex >= _machines.Count ||
        secondIndex < 0 || secondIndex >= _machines.Count)
    {
      throw new ArgumentOutOfRangeException("Indeks maszyny poza zakresem.");
    }

    // [WYMÓG 7: Interfejsy / Abstrakcja] — IConnectable
    _machines[firstIndex].ConnectTo(_machines[secondIndex].ConnectionId);
    _logQueue.Enqueue(
        $"Połączono {_machines[firstIndex].Name} → {_machines[secondIndex].ConnectionId}");
  }

  private void WireMachineEvents() =>
      AlertRaised += (_, message) => _logQueue.Enqueue($"ALERT: {message}");

  // [WYMÓG 9: Delegacje / Zdarzenia] — handler podpięty do OnOverheat
  private void HandleMachineOverheat(object? sender, OverheatEventArgs e)
  {
    string text =
        $"PRZEGRZANIE: {e.MachineName} ({e.CurrentHeat:F1}°C / limit {e.HeatLimit:F1}°C) — awaryjne wyłączenie.";
    AlertRaised?.Invoke(this, text);
    _logQueue.Enqueue(text);
  }
}
