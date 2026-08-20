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

    public async Task<ProductResponse?> GetProductByIdAsync(int productId)
    {
        var response = await _httpClient.GetAsync(
            $"/api/products/{productId}"
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<ProductResponse>();
    }

    public async Task<HttpResponseMessage> DecreaseStockAsync(
        DecreaseStockRequest request)
    {
        return await _httpClient.PostAsJsonAsync(
            "/api/products/decrease-stock",
            request
        );
    }
}