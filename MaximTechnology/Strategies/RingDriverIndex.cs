using Maxim_Technology;

namespace MaximTechnology.Strategies;

public class RingDriverIndex(Map map) : DriverIndex(map)
{
    private readonly int _m = map.Y;
    private readonly int _n = map.X;

    protected override List<Driver> FindNearestCore(OrderLocation orderLocation, int count = NearestCount)
    {
        var maxRadius = Math.Max(
            Math.Max(orderLocation.X, _n - 1 - orderLocation.X),
            Math.Max(orderLocation.Y, _m - 1 - orderLocation.Y));

        var nearest = new NearestDrivers(count);

        // любая клетка кольца r не ближе r шагов, поэтому как набрали count, досматриваем кольца, пока r меньше самого дальнего из них
        for (var r = 0; r <= maxRadius && r < nearest.Bound; r++)
            VisitRing(r);

        return nearest.ToList();

        void VisitRing(int r)
        {
            var left = orderLocation.X - r;
            var right = orderLocation.X + r;
            var bottom = orderLocation.Y - r;
            var top = orderLocation.Y + r;

            var fromX = Math.Max(left, 0);
            var toX = Math.Min(right, _n - 1);
            if (bottom >= 0)
                for (var x = fromX; x <= toX; x++)
                    Check(x, bottom);
            if (top < _m && top != bottom)
                for (var x = fromX; x <= toX; x++)
                    Check(x, top);

            var fromY = Math.Max(bottom + 1, 0);
            var toY = Math.Min(top - 1, _m - 1);
            if (left >= 0)
                for (var y = fromY; y <= toY; y++)
                    Check(left, y);
            if (right < _n && right != left)
                for (var y = fromY; y <= toY; y++)
                    Check(right, y);
        }

        void Check(int x, int y)
        {
            var driver = DriverAt(x, y);
            if (driver is null) return;

            nearest.Offer(driver, Math.Abs((long)x - orderLocation.X) + Math.Abs((long)y - orderLocation.Y));
        }
    }
}