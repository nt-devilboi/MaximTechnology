using Maxim_Technology.Strategies;

namespace Maxim_Technology;

public class RowsDriverIndex(Map map) : DriverIndex(map)
{
    // для каждой строки Y — отсортированные X водителей в ней
    private readonly List<int>[] _rows = Enumerable.Range(0, map.Y).Select(_ => new List<int>()).ToArray();

    protected override void OnAdded(Driver driver)
    {
        var row = _rows[driver.Y];
        row.Insert(~row.BinarySearch(driver.X), driver.X);
    }

    protected override void OnRemoved(Driver driver)
    {
        var row = _rows[driver.Y];
        row.RemoveAt(row.BinarySearch(driver.X));
    }

    public override List<Driver> FindNearest(OrderLocation orderLocation, int count = NearestCount)
    {
        var nearest = new NearestDrivers(count);

        // строки обходим от строки заказа вниз и вверх; в строке на offset любой водитель не ближе offset
        var maxOffset = Math.Max(orderLocation.Y, Map.Y - 1 - orderLocation.Y);
        for (var offset = 0; offset <= maxOffset && (long)offset * offset < nearest.Bound; offset++)
        {
            var below = orderLocation.Y - offset;
            var above = orderLocation.Y + offset;
            if (below >= 0) CheckRow(below);
            if (above < Map.Y && above != below) CheckRow(above);
        }

        return nearest.ToList();

        // от X заказа идём по строке в обе стороны, пока водители ближе самого дальнего из найденных
        void CheckRow(int y)
        {
            var row = _rows[y];

            var start = row.BinarySearch(orderLocation.X);
            if (start < 0) start = ~start;

            for (var i = start; i < row.Count; i++)
                if (!TryOffer(row[i], y)) break;
            for (var i = start - 1; i >= 0; i--)
                if (!TryOffer(row[i], y)) break;
        }

        // false — водитель не ближе границы, дальше по строке в эту сторону будут только дальше
        bool TryOffer(int x, int y)
        {
            var dx = (long)x - orderLocation.X;
            var dy = (long)y - orderLocation.Y;
            var distance = dx * dx + dy * dy;
            if (distance >= nearest.Bound) return false;

            nearest.Offer(DriverAt(x, y)!, distance);
            return true;
        }
    }
}
