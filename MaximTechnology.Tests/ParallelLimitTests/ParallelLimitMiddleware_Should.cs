using MaximTechnology.Web;
using Microsoft.AspNetCore.Http;

namespace MaximTechnology.Tests.ParallelLimitTests;

public class ParallelLimitMiddleware_Should
{
    [Test]
    public async Task InvokeAsync_MoreParallelRequestsThanLimit_PassesOnlyLimitOthersGet503()
    {
        const int limit = 3;
        const int requests = 100;
        var gate = new TaskCompletionSource();
        var passed = 0;
        var sut = new ParallelLimitMiddleware(_ =>
        {
            Interlocked.Increment(ref passed);
            return gate.Task;
        }, limit);
        var contexts = Enumerable.Range(0, requests).Select(_ => new DefaultHttpContext()).ToArray();
        var tasks = new Task[requests];

        Parallel.For(0, requests, i => tasks[i] = sut.InvokeAsync(contexts[i]));

        gate.SetResult();
        await Task.WhenAll(tasks);

        var rejected = contexts.Count(c => c.Response.StatusCode == StatusCodes.Status503ServiceUnavailable);
        Assert.That(passed, Is.EqualTo(limit));
        Assert.That(rejected, Is.EqualTo(requests - limit));
    }
}