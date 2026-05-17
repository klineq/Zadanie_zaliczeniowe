using SpaceBaseLogistics.Core;

namespace SpaceBaseLogistics.Models;

// [WYMÓG 7: Interfejsy / Abstrakcja] — klasa abstrakcyjna
public abstract class Machine : IConnectable
{
  private static int _nextId = 1;
  private string? _connectedTo;

  // [WYMÓG 2: Konstruktory]
  protected Machine(string name, double powerDrawKwh, double heatLimit)
  {
    Id = _nextId++;
    Name = name;
    PowerDrawKwh = powerDrawKwh;
    HeatLimit = heatLimit;
    CurrentHeat = 0;
    IsRunning = true;
    ConnectionId = $"MCH-{Id:D4}";
  }

  public int Id { get; }
  public string Name { get; }
  public double PowerDrawKwh { get; }
  public double HeatLimit { get; }
  public double CurrentHeat { get; protected set; }
  public bool IsRunning { get; protected set; }

  public string ConnectionId { get; }
  public bool IsConnected => _connectedTo is not null;

  public void ConnectTo(string targetId) => _connectedTo = targetId;

  public void Disconnect() => _connectedTo = null;

  // [WYMÓG 9: Delegacje / Zdarzenia]
  public event EventHandler<OverheatEventArgs>? OnOverheat;

  // [WYMÓG 6: Polimorfizm]
  public virtual void ProcessTick(Inventory inventory)
  {
    if (!IsRunning)
    {
      return;
    }

    BaseEnergyTracker.RegisterConsumption(PowerDrawKwh);
    CurrentHeat += PowerDrawKwh * 0.4;

    if (CurrentHeat >= HeatLimit)
    {
      TriggerOverheat();
    }
  }

  protected void TriggerOverheat()
  {
    IsRunning = false;
    OnOverheat?.Invoke(this, new OverheatEventArgs(Name, CurrentHeat, HeatLimit));
  }

  public void ForceOverheatForDemo() => TriggerOverheat();

  public void CoolDown(double amount) => CurrentHeat = Math.Max(0, CurrentHeat - amount);

  public void ApplySavedState(double currentHeat, bool isRunning)
  {
    CurrentHeat = currentHeat;
    IsRunning = isRunning;
  }

  public override string ToString() =>
      $"{Name} [#{Id}] | {StatusLabel} | Ciepło: {CurrentHeat:F1}/{HeatLimit:F1}";

  protected string StatusLabel => IsRunning ? "AKTYWNA" : "WYŁĄCZONA";
}

// [WYMÓG 9: Delegacje / Zdarzenia] — argumenty zdarzenia
public sealed class OverheatEventArgs : EventArgs
{
  public OverheatEventArgs(string machineName, double currentHeat, double heatLimit)
  {
    MachineName = machineName;
    CurrentHeat = currentHeat;
    HeatLimit = heatLimit;
  }

  public string MachineName { get; }
  public double CurrentHeat { get; }
  public double HeatLimit { get; }
}
