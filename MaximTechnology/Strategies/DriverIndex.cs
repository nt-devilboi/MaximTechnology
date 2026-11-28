using MaximTechnology.Entity;

namespace MaximTechnology.Strategies;

public abstract class DriverIndex(Map map)
{
    public const int NearestCount = 5;
    private readonly Dictionary<Guid, Driver> _drivers = [];
    private readonly Driver?[,] _grid = new Driver?[map.X, map.Y];
    private readonly object _lock = new();


    protected Map Map { get; } = map;

    protected Dictionary<Guid, Driver>.ValueCollection Drivers => _drivers.Values;


    public bool Upsert(Driver driver)
    {
        lock (_lock)
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
    }

    public bool Contains(Guid driverId)
    {
        return _drivers.ContainsKey(driverId);
    }

    public bool Remove(Guid driverId)
    {
        lock (_lock)
        {
            if (!_drivers.Remove(driverId, out var driver)) return false;

            _grid[driver.X, driver.Y] = null;
            OnRemoved(driver);
            return true;
        }
    }


    public List<Driver> FindNearest(OrderLocation order, int count = NearestCount)
    {
        lock (_lock)
        {
            return FindNearestCore(order, count);
        }
    }

    // до count ближайших по маршруту водителей (|dx| + |dy|), отсортированы от ближнего к дальнему
    protected abstract List<Driver> FindNearestCore(OrderLocation orderLocation, int count = NearestCount);

    protected Driver? DriverAt(int x, int y)
    {
        return _grid[x, y];
    }


    protected virtual void OnAdded(Driver driver)
    {
    }

    protected virtual void OnRemoved(Driver driver)
    {
    }
}