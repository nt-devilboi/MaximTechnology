using Microsoft.AspNetCore.Mvc.Rendering;

namespace MaximTechnology.Web;

public class ParallelLimitMiddleware
{
    private readonly int Limit;

    private readonly RequestDelegate next;
    private int current;

    public ParallelLimitMiddleware(RequestDelegate next, int limit)
    {
        this.next = next;
        Limit = limit;
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
            await next.Invoke(context);
        }
        finally
        {
            Interlocked.Decrement(ref current);
        }
    }

    // Можно было бы просто использовать семафор слим я понимаю, но хотелось углубиться в CAS.
    public bool TryEnter()
    {
        while (true)
        {
            var observed = Volatile.Read(ref current);
            if (observed >= Limit) return false;


            if (Interlocked.CompareExchange(ref current, observed + 1, observed) == observed) return true;
        }
    }
}