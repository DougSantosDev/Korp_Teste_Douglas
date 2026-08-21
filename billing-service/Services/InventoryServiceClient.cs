using BillingService.DTOs;
using System.Net;

namespace BillingService.Services;

public class InventoryServiceClient
{
    private readonly HttpClient _httpClient;

    public InventoryServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ProductResponse?> GetProductByIdAsync(
        int productId,
        CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(
            $"/api/products/{productId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductResponse>(cancellationToken);
    }

    public async Task<HttpResponseMessage> DecreaseStockAsync(
        DecreaseStockRequest request,
        CancellationToken cancellationToken)
    {
        const int attempts = 2;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/products/decrease-stock",
                    request,
                    cancellationToken);

                if ((int)response.StatusCode < 500 || attempt == attempts)
                {
                    return response;
                }

                response.Dispose();
            }
            catch (HttpRequestException) when (attempt < attempts)
            {
                // A retry is safe because the request carries a stable idempotency key.
            }
            catch (TaskCanceledException) when (
                !cancellationToken.IsCancellationRequested && attempt < attempts)
            {
                // Retry a dependency timeout once.
            }

            await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);
        }

        throw new InvalidOperationException("The inventory request did not produce a response.");
    }
}
