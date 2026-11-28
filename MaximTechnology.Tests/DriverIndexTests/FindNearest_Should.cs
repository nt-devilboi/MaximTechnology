using Maxim_Technology;
using MaximTechnology.Strategies;

namespace MaximTechnology.Tests.DriverIndexTests;

[TestFixtureSource(typeof(DriverIndexFixture), nameof(Indexes))]
public class FindNearest_Should(Func<Map, DriverIndex> createIndex) : DriverIndexFixture(createIndex)
{
    // (5,8) раньше (7,7): по шагам 3 < 4, а по прямой было бы наоборот
    [Test]
    public void FindNearest_MoreDriversThanCount_ReturnsFiveNearestSortedByDistance()
    {
        List<Driver> nearest = [NewDriver(5, 5), NewDriver(5, 6), NewDriver(7, 5), NewDriver(5, 8), NewDriver(7, 7)];
        List<Driver> far = [NewDriver(0, 0), NewDriver(9, 9), NewDriver(0, 9), NewDriver(9, 0)];
        var sut = CreateIndex([..far, ..nearest]);

        var result = sut.FindNearest(new OrderLocation(5, 5));

        Assert.That(result, Is.EqualTo(nearest));
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

            var expectedDistances = expected.Values.Select(d => Steps(d, order)).OrderBy(d => d)
                .Take(DriverIndex.NearestCount);
            Assert.That(result, Is.Unique);
            Assert.That(result, Has.All.Matches<Driver>(d => ReferenceEquals(expected[d.Id], d)));
            Assert.That(result.Select(d => Steps(d, order)), Is.EqualTo(expectedDistances));
        }
    }

    [Test]
    public void FindNearest_NoDrivers_ReturnsEmpty()
    {
        var sut = CreateIndex();

        var result = sut.FindNearest(new OrderLocation(5, 5));

        Assert.That(result, Is.Empty);
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
    public void FindNearest_OnlyDriverInOppositeCorner_ReturnsIt()
    {
        var driver = NewDriver(9, 0);
        var sut = CreateIndex(driver);

        var result = sut.FindNearest(new OrderLocation(0, 9));

        Assert.That(result, Is.EqualTo(new[] { driver }));
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
    public void FindNearest_EqualDistancesOnBoundary_ReturnsAnyOfTied()
    {
        List<Driver> closer = [NewDriver(5, 5), NewDriver(5, 6), NewDriver(6, 5)];
        List<Driver> tied = [NewDriver(5, 7), NewDriver(7, 5), NewDriver(3, 5), NewDriver(6, 6)];
        var sut = CreateIndex([NewDriver(0, 0), ..tied, ..closer, NewDriver(9, 9)]);

        var result = sut.FindNearest(new OrderLocation(5, 5));

        Assert.That(result, Has.Count.EqualTo(DriverIndex.NearestCount));
        Assert.That(result.Take(closer.Count), Is.EquivalentTo(closer));
        Assert.That(tied, Is.SupersetOf(result.Skip(closer.Count)));
    }

    private static long Steps(Driver driver, OrderLocation order)
    {
        return Math.Abs((long)driver.X - order.X) + Math.Abs((long)driver.Y - order.Y);
    }
}