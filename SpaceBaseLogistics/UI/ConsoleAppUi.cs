using Spectre.Console;
using SpaceBaseLogistics.Core;
using SpaceBaseLogistics.Models;
using SpaceBaseLogistics.Services;

namespace SpaceBaseLogistics.UI;

// [WYMÓG 1: Klasy]
public sealed class ConsoleAppUi
{
  private readonly SpaceBaseSimulator _simulator;
  private readonly StatePersistenceService _persistence;
  private readonly PlanetScanService _planetScan;
  private readonly DiagnosticReflectionService _diagnostics;

  public ConsoleAppUi(
      SpaceBaseSimulator simulator,
      StatePersistenceService persistence,
      PlanetScanService planetScan,
      DiagnosticReflectionService diagnostics)
  {
    _simulator = simulator;
    _persistence = persistence;
    _planetScan = planetScan;
    _diagnostics = diagnostics;

    _simulator.AlertRaised += (_, message) =>
        AnsiConsole.MarkupLine($"[red bold]⚠ {Markup.Escape(message)}[/]");
  }

  // [WYMÓG 11: Programowanie asynchroniczne] + Spectre.Console (ocena 5.0)
  public async Task RunAsync(CancellationToken cancellationToken = default)
  {
    ShowWelcome();
    SeedDemoIfEmpty();

    bool running = true;
    while (running && !cancellationToken.IsCancellationRequested)
    {
      RenderDashboard();
      string choice = AnsiConsole.Prompt(
          new SelectionPrompt<string>()
              .Title("[bold cyan]Główne menu[/]")
              .PageSize(12)
              .AddChoices(
                  "Dodaj maszynę",
                  "Wykonaj cykl pracy (tick)",
                  "Symuluj awarię (przegrzanie)",
                  "Połącz dwie maszyny (IConnectable)",
                  "Scal paczki surowca (operator +)",
                  "Skan planety (async + pasek postępu)",
                  "Zapisz stan bazy (JSON async)",
                  "Wczytaj stan bazy (JSON async)",
                  "Diagnostyka refleksyjna maszyny",
                  "Statystyki i magazyn",
                  "Wyjście"));

      try
      {
        running = choice switch
        {
          "Dodaj maszynę" => await HandleAddMachineAsync(),
          "Wykonaj cykl pracy (tick)" => HandleTick(),
          "Symuluj awarię (przegrzanie)" => HandleOverheat(),
          "Połącz dwie maszyny (IConnectable)" => HandleConnect(),
          "Scal paczki surowca (operator +)" => HandleMergeStacks(),
          "Skan planety (async + pasek postępu)" => await HandlePlanetScanAsync(cancellationToken),
          "Zapisz stan bazy (JSON async)" => await HandleSaveAsync(cancellationToken),
          "Wczytaj stan bazy (JSON async)" => await HandleLoadAsync(cancellationToken),
          "Diagnostyka refleksyjna maszyny" => HandleReflectionDiagnostics(),
          "Statystyki i magazyn" => HandleStats(),
          "Wyjście" => false,
          _ => true
        };
      }
      catch (Exception ex)
      {
        AnsiConsole.MarkupLine($"[red]Błąd: {Markup.Escape(ex.Message)}[/]");
      }

      if (running)
      {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Naciśnij Enter, aby wrócić do menu...[/]");
        Console.ReadLine();
      }
    }

    AnsiConsole.MarkupLine("[green]Do zobaczenia, operatorze bazy![/]");
  }

  private void ShowWelcome()
  {
    AnsiConsole.Write(new FigletText("Space Base").Color(Color.Cyan1));
    AnsiConsole.MarkupLine("[bold]Symulator Logistyki Bazy Kosmicznej[/]");
    AnsiConsole.MarkupLine("[dim]Projekt PO — wszystkie mechanizmy oznaczone komentarzami [WYMÓG n][/]\n");
  }

  private void SeedDemoIfEmpty()
  {
    if (_simulator.Machines.Count > 0)
    {
      return;
    }

    _simulator.AddMachine(new Miner("Koparka Alfa"));
    _simulator.AddMachine(new Smelter("Huta Beta"));
    _simulator.Inventory[Miner.IronOre.Name] = 10;

    AnsiConsole.MarkupLine("[yellow]Załadowano demo: Miner + Smelter + 10 kg rudy.[/]\n");
  }

  private void RenderDashboard()
  {
    AnsiConsole.Clear();

    var panel = new Panel(
        $"[bold]{Markup.Escape(_simulator.BaseName)}[/]\n" +
        $"Energia magazynowa: [green]{_simulator.StoredEnergyKwh:F1} kWh[/]\n" +
        $"Suma zużycia (statyczny licznik): [yellow]{BaseEnergyTracker.TotalConsumedKwh:F2} kWh[/] " +
        $"([dim]{BaseEnergyTracker.ToMegajoules(BaseEnergyTracker.TotalConsumedKwh):F2} MJ[/])")
    {
      Header = new PanelHeader("Panel bazy"),
      Border = BoxBorder.Rounded
    };

    AnsiConsole.Write(panel);
    AnsiConsole.WriteLine();

    var machineTable = new Table().Border(TableBorder.Rounded);
    machineTable.AddColumn("ID");
    machineTable.AddColumn("Nazwa");
    machineTable.AddColumn("Typ");
    machineTable.AddColumn("Status");
    machineTable.AddColumn("Ciepło");
    machineTable.AddColumn("Połączenie");

    foreach (Machine machine in _simulator.Machines)
    {
      double heatPercent = machine.HeatLimit > 0
          ? machine.CurrentHeat / machine.HeatLimit
          : 0;

      machineTable.AddRow(
          machine.Id.ToString(),
          machine.Name,
          machine.GetType().Name,
          machine.IsRunning ? "[green]ON[/]" : "[red]OFF[/]",
          $"{machine.CurrentHeat:F0}/{machine.HeatLimit:F0}",
          machine.IsConnected ? "[cyan]TAK[/]" : "[dim]nie[/]");
    }

    if (_simulator.Machines.Count == 0)
    {
      machineTable.AddRow("-", "-", "-", "-", "-", "-");
    }

    AnsiConsole.Write(machineTable);
    AnsiConsole.WriteLine();
  }

  private Task<bool> HandleAddMachineAsync()
  {
    string type = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Wybierz typ maszyny")
            .AddChoices("Miner (wydobycie)", "Smelter (przetapianie)"));

    string name = AnsiConsole.Ask<string>("Nazwa maszyny:", $"Jednostka-{Random.Shared.Next(100, 999)}");

    Machine machine = type.StartsWith("Miner", StringComparison.Ordinal)
        ? new Miner(name)
        : new Smelter(name);

    _simulator.AddMachine(machine);
    AnsiConsole.MarkupLine($"[green]Dodano:[/] {machine}");
    return Task.FromResult(true);
  }

  private bool HandleTick()
  {
    AnsiConsole.MarkupLine("[cyan]Uruchamiam cykl produkcyjny...[/]");

    double energyBefore = BaseEnergyTracker.TotalConsumedKwh;
    _simulator.RunTick();
    double tickUsage = BaseEnergyTracker.TotalConsumedKwh - energyBefore;

    foreach (Machine machine in _simulator.Machines)
    {
      double ratio = machine.HeatLimit > 0 ? machine.CurrentHeat / machine.HeatLimit : 0;
      AnsiConsole.Progress()
          .Start(ctx =>
          {
            var task = ctx.AddTask($"[yellow]{machine.Name}[/]", maxValue: 100);
            task.Value = Math.Min(100, ratio * 100);
          });
    }

    AnsiConsole.MarkupLine($"[green]Tick OK.[/] Zużycie w tym cyklu: [yellow]{tickUsage:F2} kWh[/]");
    ShowInventorySnippet();
    return true;
  }

  private bool HandleOverheat()
  {
    AnsiConsole.MarkupLine("[red]Wymuszam przegrzanie (demo eventu OnOverheat)...[/]");
    _simulator.TriggerDemoOverheat();
    return true;
  }

  private bool HandleConnect()
  {
    if (_simulator.Machines.Count < 2)
    {
      AnsiConsole.MarkupLine("[yellow]Potrzebujesz co najmniej dwóch maszyn.[/]");
      return true;
    }

    int first = AnsiConsole.Prompt(new SelectionPrompt<int>()
        .Title("Maszyna źródłowa")
        .UseConverter(i => FormatMachineChoice(i))
        .AddChoices(Enumerable.Range(0, _simulator.Machines.Count)));

    int second = AnsiConsole.Prompt(new SelectionPrompt<int>()
        .Title("Maszyna docelowa")
        .UseConverter(i => FormatMachineChoice(i))
        .AddChoices(Enumerable.Range(0, _simulator.Machines.Count).Where(i => i != first)));

    _simulator.ConnectMachines(first, second);
    AnsiConsole.MarkupLine("[green]Połączenie kablowe nawiązane (IConnectable).[/]");
    return true;
  }

  private string FormatMachineChoice(int index)
  {
    Machine m = _simulator.Machines[index];
    return $"#{m.Id} {m.Name} ({m.GetType().Name})";
  }

  private bool HandleMergeStacks()
  {
    var stackA = new ResourceStack(Miner.IronOre, AnsiConsole.Ask<double>("Paczka A (kg):", 5));
    var stackB = new ResourceStack(Miner.IronOre, AnsiConsole.Ask<double>("Paczka B (kg):", 3));

    // [WYMÓG 10: Przeciążanie operatorów] — użycie w UI
    ResourceStack merged = stackA + stackB;

    _simulator.Inventory[Miner.IronOre.Name] += merged.Amount;
    AnsiConsole.MarkupLine($"[green]Scalono paczki:[/] {merged}");
    return true;
  }

  private async Task<bool> HandlePlanetScanAsync(CancellationToken cancellationToken)
  {
    AnsiConsole.MarkupLine("[cyan]Głębokie skanowanie planety...[/]");

    string result = await AnsiConsole.Progress()
        .StartAsync(async ctx =>
        {
          ProgressTask task = ctx.AddTask("[green]Skanowanie[/]", maxValue: 100);
          var scanProgress = new Progress<double>(p => task.Value = p * 100);

          return await _planetScan.RunDeepScanAsync(scanProgress, cancellationToken);
        });

    AnsiConsole.MarkupLine($"[bold green]{Markup.Escape(result)}[/]");
    return true;
  }

  private async Task<bool> HandleSaveAsync(CancellationToken cancellationToken)
  {
    await _persistence.SaveAsync(_simulator, cancellationToken);
    AnsiConsole.MarkupLine($"[green]Zapisano stan do pliku JSON.[/]");
    return true;
  }

  private async Task<bool> HandleLoadAsync(CancellationToken cancellationToken)
  {
    SpaceBaseState? state = await _persistence.LoadAsync(cancellationToken);
    if (state is null)
    {
      AnsiConsole.MarkupLine("[yellow]Brak zapisanego pliku — nic do wczytania.[/]");
      return true;
    }

    _persistence.ApplyLoadedState(_simulator, state);

    AnsiConsole.MarkupLine(
        $"[green]Wczytano:[/] {state.Machines.Count} maszyn, " +
        $"{state.Inventory.Count} pozycji w magazynie.");
    return true;
  }

  private bool HandleReflectionDiagnostics()
  {
    if (_simulator.Machines.Count == 0)
    {
      AnsiConsole.MarkupLine("[yellow]Brak maszyn do diagnostyki.[/]");
      return true;
    }

    int index = AnsiConsole.Prompt(new SelectionPrompt<int>()
        .Title("Wybierz maszynę (refleksja)")
        .UseConverter(FormatMachineChoice)
        .AddChoices(Enumerable.Range(0, _simulator.Machines.Count)));

    Machine machine = _simulator.Machines[index];
    IReadOnlyList<string> lines = _diagnostics.InspectMachine(machine);

    var table = new Table().Title("[bold]Menu diagnostyczne (Reflection)[/]").Border(TableBorder.Simple);
    table.AddColumn("Właściwość / informacja");

    foreach (string line in lines)
    {
      table.AddRow(Markup.Escape(line));
    }

    AnsiConsole.Write(table);
    return true;
  }

  private bool HandleStats()
  {
    ShowInventorySnippet();

    AnsiConsole.MarkupLine(
        $"\n[bold]Energia:[/] { _simulator.StoredEnergyKwh:F1} kWh | " +
        $"[bold]Łączne zużycie:[/] {BaseEnergyTracker.TotalConsumedKwh:F2} kWh " +
        $"({BaseEnergyTracker.ToMegajoules(BaseEnergyTracker.TotalConsumedKwh):F2} MJ)");

    AnsiConsole.MarkupLine("\n[bold]Ostatnie wpisy kolejki produkcyjnej:[/]");
    foreach (string entry in _simulator.LogQueue.PeekAll().TakeLast(8))
    {
      AnsiConsole.MarkupLine($"  [dim]•[/] {Markup.Escape(entry)}");
    }

    return true;
  }

  private void ShowInventorySnippet()
  {
    var table = new Table().Title("Magazyn (indeksator inventory[name])").Border(TableBorder.Minimal);
    table.AddColumn("Surowiec");
    table.AddColumn("Ilość");

    foreach (Resource resource in KnownResources.All)
    {
      // [WYMÓG 3: Właściwości / Indeksatory]
      double amount = _simulator.Inventory[resource.Name];
      table.AddRow(resource.Name, $"{amount:F1} {resource.Unit}");
    }

    AnsiConsole.Write(table);
  }
}
