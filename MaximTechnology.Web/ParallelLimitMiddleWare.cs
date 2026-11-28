using Microsoft.AspNetCore.Mvc.Rendering;

namespace MaximTechnology.Web;

public class ParallelLimitMiddleware
{
    private readonly int _limit;

    private readonly RequestDelegate _next;
    private int _current;

    public ParallelLimitMiddleware(RequestDelegate next, int limit)
    {
        _next = next;
        _limit = limit;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!TryEnter())
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            return;
        }

        try
        {
            await _next.Invoke(context);
        }
        finally
        {
            Interlocked.Decrement(ref _current);
        }
    }

    // Можно было бы просто использовать семафор слим я понимаю, но хотелось углубиться в CAS.
    public bool TryEnter()
    {
        while (true)
        {
            var observed = Volatile.Read(ref _current);
            if (observed >= _limit) return false;


            if (Interlocked.CompareExchange(ref _current, observed + 1, observed) == observed) return true;
        }
    }
}