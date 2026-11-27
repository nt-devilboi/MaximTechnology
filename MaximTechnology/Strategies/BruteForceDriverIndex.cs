using Maxim_Technology.Strategies;

namespace Maxim_Technology;

public class BruteForceDriverIndex(Map map) : DriverIndex(map)
{
    protected override List<Driver> FindNearestCore(OrderLocation orderLocation, int count = NearestCount)
    {
        var nearest = new NearestDrivers(count);

        foreach (var driver in Drivers)
            nearest.Offer(driver, Math.Abs((long)driver.X - orderLocation.X) + Math.Abs((long)driver.Y - orderLocation.Y));

        return nearest.ToList();
    }
}
