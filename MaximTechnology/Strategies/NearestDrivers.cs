namespace Maxim_Technology.Strategies;

public class NearestDrivers(int count)
{
    private readonly PriorityQueue<Driver, long> _heap = new(count + 1);

    public long Bound => _heap.Count == count && _heap.TryPeek(out _, out var priority) ? -priority : long.MaxValue;

    public void Offer(Driver driver, long distance)
    {
        if (_heap.Count < count)
        {
            _heap.Enqueue(driver, -distance);
            return;
        }

        if (distance < Bound) _heap.EnqueueDequeue(driver, -distance);
    }

    // в приоритете лежит -distance, поэтому ближние — с наибольшим приоритетом
    public List<Driver> ToList() =>
        _heap.UnorderedItems
            .OrderByDescending(item => item.Priority)
            .Select(item => item.Element)
            .ToList();
}