using Maxim_Technology.Strategies;

namespace Maxim_Technology;

public class BruteForceDriverIndex(Map map) : DriverIndex(map)
{
    public override List<Driver> FindNearest(OrderLocation orderLocation, int count = NearestCount)
    {
        var nearest = new NearestDrivers(count);

        foreach (var driver in Drivers)
            nearest.Offer(driver, SquaredDistance(driver));

        return nearest.ToList();

        long SquaredDistance(Driver driver)
        {
            var dx = (long)driver.X - orderLocation.X;
            var dy = (long)driver.Y - orderLocation.Y;
            return dx * dx + dy * dy;
        }
    }
}
