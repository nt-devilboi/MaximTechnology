using MaximTechnology.Entity;

namespace MaximTechnology.Strategies;

public interface INearestSearch
{
    List<Driver> Find(DriverIndex index, OrderLocation orderLocation, int count = DriverIndex.NearestCount);
}
