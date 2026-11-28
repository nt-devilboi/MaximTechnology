using MaximTechnology.Entity;
using MaximTechnology.Strategies;

namespace MaximTechnology.Tests.DriverIndexTests;

public abstract class DriverIndexFixture
{
    protected static readonly Map DefaultMap = new() { X = 10, Y = 10 };

    public static IEnumerable<TestFixtureData> Searches()
    {
        return
        [
            Search(new BruteForceSearch(), nameof(BruteForceSearch)),
            Search(new RingSearch(), nameof(RingSearch))
        ];
    }

    protected static Driver NewDriver(int x, int y)
    {
        return new Driver(Guid.NewGuid(), x, y);
    }

    protected static DriverIndex CreateIndex(params Driver[] drivers)
    {
        return CreateIndex(DefaultMap, drivers);
    }

    protected static DriverIndex CreateIndex(Map map, IEnumerable<Driver> drivers)
    {
        var index = new DriverIndex(map);
        foreach (var driver in drivers) index.Upsert(driver);

        return index;
    }

    private static TestFixtureData Search(INearestSearch search, string name)
    {
        return new TestFixtureData(search).SetArgDisplayNames(name);
    }
}
