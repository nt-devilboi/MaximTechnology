namespace MaximTechnology.Strategies;

internal static class NearestSearchFactory
{
    private static readonly Dictionary<SearchKind, INearestSearch> Searches = new()
    {
        [SearchKind.BruteForce] = new BruteForceSearch(),
        [SearchKind.Ring] = new RingSearch()
    };

    public static INearestSearch Get(SearchKind kind)
    {
        return Searches[kind];
    }
}