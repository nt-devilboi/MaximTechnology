using BenchmarkDotNet.Attributes;
using Maxim_Technology;
using Maxim_Technology.Strategies;

namespace MaximTechnology.Benchmarks;

[MemoryDiagnoser]
public class FindNearestBenchmark
{
    private const int OrdersCount = 1_000;

    private OrderLocation[] _orders = [];
    private BruteForceDriverIndex _bruteForce = null!;
    private RingDriverIndex _ring = null!;
    private RowsDriverIndex _rows = null!;

    [Params(1_000)] public int MapSize { get; set; }

    [Params(100, 10_000, 100_000)] public int DriversCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var map = new Map { X = MapSize, Y = MapSize };
        var random = new Random(42);
        _bruteForce = new BruteForceDriverIndex(map);
        _ring = new RingDriverIndex(map);
        _rows = new RowsDriverIndex(map);

        // Random.Shuffle есть только с .NET 8
        var cells = Enumerable.Range(0, MapSize * MapSize).OrderBy(_ => random.Next()).ToArray();
        for (var id = 0; id < DriversCount; id++)
        {
            var driver = new Driver(Guid.NewGuid(), cells[id] / MapSize, cells[id] % MapSize);
            _bruteForce.Upsert(driver);
            _ring.Upsert(driver);
            _rows.Upsert(driver);
        }

        _orders = Enumerable.Range(0, OrdersCount)
            .Select(_ => new OrderLocation(random.Next(MapSize), random.Next(MapSize)))
            .ToArray();
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = OrdersCount)]
    public List<Driver> BruteForce() => FindForAllOrders(_bruteForce);

    [Benchmark(OperationsPerInvoke = OrdersCount)]
    public List<Driver> Ring() => FindForAllOrders(_ring);

    [Benchmark(OperationsPerInvoke = OrdersCount)]
    public List<Driver> Rows() => FindForAllOrders(_rows);

    private List<Driver> FindForAllOrders(DriverIndex index)
    {
        List<Driver> last = [];
        foreach (var order in _orders)
            last = index.FindNearest(order);

        return last;
    }
}