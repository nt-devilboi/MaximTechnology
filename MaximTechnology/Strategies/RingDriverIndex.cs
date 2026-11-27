namespace Maxim_Technology.Strategies;

public class RingDriverIndex(Map map) : DriverIndex(map)
{
    private readonly int _n = map.X;
    private readonly int _m = map.Y;

    public override List<Driver> FindNearest(OrderLocation orderLocation, int count = NearestCount)
    {
        var maxRadius = Math.Max(
            Math.Max(orderLocation.X, _n - 1 - orderLocation.X),
            Math.Max(orderLocation.Y, _m - 1 - orderLocation.Y));

        var nearest = new NearestDrivers(count);

        // любая клетка кольца r не ближе r, поэтому как набрали count, досматриваем кольца, пока r² меньше самого дальнего из них
        for (var r = 0; r <= maxRadius && (long)r * r < nearest.Bound; r++)
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

            var dx = (long)x - orderLocation.X;
            var dy = (long)y - orderLocation.Y;
            nearest.Offer(driver, dx * dx + dy * dy);
        }
    }
}
