namespace SpaceBaseLogistics.Core;

// [WYMÓG 4: Statyczne]
public static class BaseEnergyTracker
{
    private static double _totalConsumedKwh;

    public static double TotalConsumedKwh => _totalConsumedKwh;

    public static void RegisterConsumption(double kwh)
    {
        if (kwh < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(kwh));
        }

        _totalConsumedKwh += kwh;
    }

    // [WYMÓG 4: Statyczne] — przelicznik jednostek energii
    public static double ToMegajoules(double kwh) => kwh * 3.6;

    public static void Reset() => _totalConsumedKwh = 0;
}
