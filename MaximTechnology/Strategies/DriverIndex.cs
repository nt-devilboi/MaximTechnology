namespace Maxim_Technology;

public abstract class DriverIndex(Map map)
{
    public const int NearestCount = 5;
    private readonly Driver?[,] _grid = new Driver?[map.X, map.Y];
    private readonly Dictionary<Guid, Driver> _drivers = [];

    protected Map Map { get; } = map;

    protected Dictionary<Guid, Driver>.ValueCollection Drivers => _drivers.Values;


    public bool Upsert(Driver driver)
    {
        var occupant = _grid[driver.X, driver.Y];
        if (occupant is not null && occupant.Id != driver.Id) return false;

        if (_drivers.TryGetValue(driver.Id, out var old))
        {
            _grid[old.X, old.Y] = null;
            OnRemoved(old);
        }

        _grid[driver.X, driver.Y] = driver;
        _drivers[driver.Id] = driver;
        OnAdded(driver);
        return true;
    }

    public bool Remove(Guid driverId)
    {
        if (!_drivers.Remove(driverId, out var driver)) return false;

        _grid[driver.X, driver.Y] = null;
        OnRemoved(driver);
        return true;
    }


    // до count ближайших водителей, отсортированы от ближнего к дальнему
    public abstract List<Driver> FindNearest(OrderLocation orderLocation, int count = NearestCount);

    protected Driver? DriverAt(int x, int y) => _grid[x, y];


    protected virtual void OnAdded(Driver driver)
    {
    }

    protected virtual void OnRemoved(Driver driver)
    {
    }
}