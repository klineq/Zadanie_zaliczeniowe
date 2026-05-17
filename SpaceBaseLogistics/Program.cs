using SpaceBaseLogistics.Services;
using SpaceBaseLogistics.UI;

namespace SpaceBaseLogistics;

public static class Program
{
  // [WYMÓG 11: Programowanie asynchroniczne]
  public static async Task Main()
  {
    string savePath = Path.Combine(AppContext.BaseDirectory, "spacebase_save.json");

    var simulator = new SpaceBaseSimulator();
    var persistence = new StatePersistenceService(savePath);
    var planetScan = new PlanetScanService();
    var diagnostics = new DiagnosticReflectionService();

    var ui = new ConsoleAppUi(simulator, persistence, planetScan, diagnostics);

    using var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (_, e) =>
    {
      e.Cancel = true;
      cts.Cancel();
    };

    await ui.RunAsync(cts.Token);
  }
}
