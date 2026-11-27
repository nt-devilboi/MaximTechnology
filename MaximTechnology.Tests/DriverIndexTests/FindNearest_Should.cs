using Maxim_Technology;

namespace MaximTechnology.Tests.DriverIndexTests;

[TestFixtureSource(typeof(DriverIndexFixture), nameof(Indexes))]
public class FindNearest_Should(Func<Map, DriverIndex> createIndex) : DriverIndexFixture(createIndex)
{
    [Test]
    public void FindNearest_NoDrivers_ReturnsEmpty()
    {
        var sut = CreateIndex();

        var result = sut.FindNearest(new OrderLocation(5, 5));

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void FindNearest_SingleDriver_ReturnsIt()
    {
        var driver = NewDriver(9, 0);
        var sut = CreateIndex(driver);

        var result = sut.FindNearest(new OrderLocation(0, 9));

        Assert.That(result, Is.EqualTo(new[] { driver }));
    }

    [Test]
    public void FindNearest_FewerDriversThanCount_ReturnsAllSortedByDistance()
    {
        var first = NewDriver(5, 6);
        var second = NewDriver(7, 5);
        var third = NewDriver(0, 0);
        var sut = CreateIndex(third, first, second);

        var result = sut.FindNearest(new OrderLocation(5, 5));

        Assert.That(result, Is.EqualTo(new[] { first, second, third }));
    }

    [Test]
    public void FindNearest_MoreDriversThanCount_ReturnsFiveNearestSortedByDistance()
    {
        List<Driver> nearest = [NewDriver(5, 5), NewDriver(5, 6), NewDriver(7, 5), NewDriver(7, 7), NewDriver(5, 8)];
        List<Driver> far = [NewDriver(0, 0), NewDriver(9, 9), NewDriver(0, 9), NewDriver(9, 0)];
        var sut = CreateIndex([..far, ..nearest]);

        var result = sut.FindNearest(new OrderLocation(5, 5));

        Assert.That(result, Is.EqualTo(nearest));
    }

    [TestCase(1)]
    [TestCase(3)]
    public void FindNearest_CustomCount_ReturnsThatMany(int count)
    {
        var sut = CreateIndex(NewDriver(1, 1), NewDriver(2, 2), NewDriver(3, 3), NewDriver(4, 4), NewDriver(8, 8));

        var result = sut.FindNearest(new OrderLocation(0, 0), count);

        Assert.That(result, Has.Count.EqualTo(count));
    }

    [Test]
    public void FindNearest_DriverOnOrderCell_ReturnsItFirst()
    {
        var onOrderCell = NewDriver(3, 4);
        var sut = CreateIndex(NewDriver(3, 5), onOrderCell, NewDriver(2, 4));

        var result = sut.FindNearest(new OrderLocation(3, 4));

        Assert.That(result[0], Is.EqualTo(onOrderCell));
    }

    [Test]
    public void FindNearest_DiagonalCloserThanStraight_UsesEuclideanDistance()
    {
        var diagonal = NewDriver(2, 2);
        var straight = NewDriver(3, 0);
        var sut = CreateIndex(straight, diagonal);

        var result = sut.FindNearest(new OrderLocation(0, 0));

        Assert.That(result, Is.EqualTo(new[] { diagonal, straight }));
    }

    [Test]
    public void FindNearest_StraightCloserThanDiagonal_UsesEuclideanDistance()
    {
        var straight = NewDriver(9, 5);
        var diagonal = NewDriver(8, 8);
        var sut = CreateIndex(diagonal, straight);

        var result = sut.FindNearest(new OrderLocation(5, 5));

        Assert.That(result, Is.EqualTo(new[] { straight, diagonal }));
    }

    [Test]
    public void FindNearest_EqualDistancesOnBoundary_ReturnsAnyOfTied()
    {
        List<Driver> closer = [NewDriver(5, 5), NewDriver(5, 6), NewDriver(6, 5)];
        List<Driver> tied = [NewDriver(5, 7), NewDriver(7, 5), NewDriver(3, 5)];
        var sut = CreateIndex([NewDriver(0, 0), ..tied, ..closer, NewDriver(9, 9)]);

        var result = sut.FindNearest(new OrderLocation(5, 5));

        Assert.That(result, Has.Count.EqualTo(DriverIndex.NearestCount));
        Assert.That(result.Take(closer.Count), Is.EquivalentTo(closer));
        Assert.That(tied, Is.SupersetOf(result.Skip(closer.Count)));
    }

    [TestCase(1, 500)]
    [TestCase(42, 20)]
    [TestCase(7, 4)]
    [TestCase(2026, 1)]
    public void FindNearest_RandomDrivers_ReturnsDriversWithMinimalDistances(int seed, int driversCount)
    {
        var map = new Map { X = 100, Y = 80 };
        var random = new Random(seed);
        var drivers = Enumerable.Range(0, map.X * map.Y)
            .OrderBy(_ => random.Next())
            .Take(driversCount)
            .Select(cell => NewDriver(cell / map.Y, cell % map.Y))
            .ToList();
        var order = new OrderLocation(random.Next(map.X), random.Next(map.Y));
        var sut = CreateIndex(map, drivers);

        var result = sut.FindNearest(order);

        Assert.That(result, Is.Unique);
        Assert.That(result.Select(d => SquaredDistance(d, order)), Is.EqualTo(ExpectedDistances(drivers, order)));
    }

    // много добавлений, перемещений и удалений подряд - свои структуры алгоритмов не должны разойтись с сеткой
    [TestCase(7)]
    [TestCase(99)]
    public void FindNearest_AfterRandomMovesAndRemoves_ReturnsDriversWithMinimalDistances(int seed)
    {
        var map = new Map { X = 60, Y = 40 };
        var random = new Random(seed);
        var ids = Enumerable.Range(0, 300).Select(_ => Guid.NewGuid()).ToArray();
        var sut = CreateIndex(map, []);
        var expected = new Dictionary<Guid, Driver>();
        for (var step = 0; step < 3000; step++)
        {
            var id = ids[random.Next(ids.Length)];
            if (random.Next(3) == 0)
            {
                if (sut.Remove(id)) expected.Remove(id);
                continue;
            }

            var driver = new Driver(id, random.Next(map.X), random.Next(map.Y));
            if (sut.Upsert(driver)) expected[id] = driver;
        }

        for (var i = 0; i < 30; i++)
        {
            var order = new OrderLocation(random.Next(map.X), random.Next(map.Y));

            var result = sut.FindNearest(order);

            Assert.That(result, Is.Unique);
            Assert.That(result, Has.All.Matches<Driver>(d => ReferenceEquals(expected[d.Id], d)));
            Assert.That(result.Select(d => SquaredDistance(d, order)),
                Is.EqualTo(ExpectedDistances(expected.Values, order)));
        }
    }

    private static IEnumerable<long> ExpectedDistances(IEnumerable<Driver> drivers, OrderLocation order) =>
        drivers.Select(d => SquaredDistance(d, order)).OrderBy(d => d).Take(DriverIndex.NearestCount);

    private static long SquaredDistance(Driver driver, OrderLocation order)
    {
        var dx = (long)driver.X - order.X;
        var dy = (long)driver.Y - order.Y;
        return dx * dx + dy * dy;
    }
}
