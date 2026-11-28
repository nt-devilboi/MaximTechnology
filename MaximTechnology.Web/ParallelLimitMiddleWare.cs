namespace MaximTechnology.Web;

public class ParallelLimitMiddleware
{
    private int current = 0;

    private readonly RequestDelegate next;
    private readonly int Limit;

    public ParallelLimitMiddleware(RequestDelegate next, int limit)
    {
        this.next = next;
        Limit = limit;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (Interlocked.Increment(ref current) > Limit)
        {
            Interlocked.Decrement(ref current);
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
}