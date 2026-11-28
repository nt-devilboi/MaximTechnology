using System.Net.Http.Json;
using System.Text.Json;

namespace MaximTechnology;

public class RandomNumber(HttpClient httpClient)
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);

    public async Task<int> Next(int max)
    {
        try
        {
            using var cts = new CancellationTokenSource(Timeout);
            var numbers = await httpClient.GetFromJsonAsync<int[]>(
                $"https://www.randomnumberapi.com/api/v1.0/random?min=0&max={max}&count=1&secure=true", cts.Token);

            if (numbers is [var number and >= 0] && number < max) return number;
        }
        catch (Exception e) when (e is HttpRequestException or OperationCanceledException or JsonException
                                      or NotSupportedException)
        {
        }

        return Random.Shared.Next(max);
    }
}