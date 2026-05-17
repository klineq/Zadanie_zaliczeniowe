namespace SpaceBaseLogistics.Services;

// [WYMÓG 1: Klasy]
public sealed class PlanetScanService
{
  // [WYMÓG 11: Programowanie asynchroniczne]
  public async Task<string> RunDeepScanAsync(
      IProgress<double>? progress = null,
      CancellationToken cancellationToken = default)
  {
    const int steps = 20;

    for (int i = 1; i <= steps; i++)
    {
      cancellationToken.ThrowIfCancellationRequested();
      await Task.Delay(120, cancellationToken);
      progress?.Report(i / (double)steps);
    }

    return "Wykryto złoża żelaza na współrzędnych X:42 / Y:17 — zalecany Miner.";
  }
}
