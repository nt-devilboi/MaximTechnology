using MaximTechnology.Entity;
using MaximTechnology.Strategies;

namespace MaximTechnology.Tests.DriverIndexTests;

public abstract class DriverIndexFixture(Func<Map, DriverIndex> createIndex)
{
    protected static readonly Map DefaultMap = new() { X = 10, Y = 10 };

    public static IEnumerable<TestFixtureData> Indexes()
    {
        return
        [
            Index(map => new BruteForceDriverIndex(map), nameof(BruteForceDriverIndex)),
            Index(map => new RingDriverIndex(map), nameof(RingDriverIndex)),
            Index(map => new RowsDriverIndex(map), nameof(RowsDriverIndex))
        ];
    }

    protected static Driver NewDriver(int x, int y)
    {
        return new Driver(Guid.NewGuid(), x, y);
    }

    protected DriverIndex CreateIndex(params Driver[] drivers)
    {
        return CreateIndex(DefaultMap, drivers);
    }

    protected DriverIndex CreateIndex(Map map, IEnumerable<Driver> drivers)
    {
        var index = createIndex(map);
        foreach (var driver in drivers) index.Upsert(driver);

        return index;
    }

    private static TestFixtureData Index(Func<Map, DriverIndex> factory, string name)
    {
        return new TestFixtureData(factory).SetArgDisplayNames(name);
    }
}