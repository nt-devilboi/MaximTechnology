using MaximTechnology.Entity;

namespace MaximTechnology.Strategies;

public class BruteForceSearch : INearestSearch
{
    public List<Driver> Find(DriverIndex index, OrderLocation orderLocation, int count)
    {
        var nearest = new NearestDrivers(count);

        foreach (var driver in index.Drivers)
            nearest.Offer(driver,
                Math.Abs((long)driver.X - orderLocation.X) + Math.Abs((long)driver.Y - orderLocation.Y));

        return nearest.ToList();
    }
}
