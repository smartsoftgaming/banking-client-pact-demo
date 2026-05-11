using BankingPlatform.Client.Exceptions;
using BankingPlatform.Client.Models;
using System.Net;
using System.Net.Http.Json;

namespace BankingPlatform.Client.Clients;

public interface IAccountApiClient
{
    Task<decimal> GetBalanceAsync(int accountId, CancellationToken cancellationToken = default);
    Task<decimal> GetOverdraftAsync(int accountId, CancellationToken cancellationToken = default);
}

public class AccountApiClient(HttpClient httpClient) : IAccountApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<decimal> GetBalanceAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/accounts/{accountId}/balance", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: cancellationToken);
            throw new NotFoundException(error?.Message ?? "Account not found");
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<BalanceResponse>(cancellationToken: cancellationToken);

        return result?.Balance ?? throw new InvalidOperationException("Balance response was empty.");
    }

    public async Task<decimal> GetOverdraftAsync(int accountId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/accounts/{accountId}/overdraft", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: cancellationToken);
            throw new NotFoundException(error?.Message ?? "Account not found");
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OverdraftResponse>(cancellationToken: cancellationToken) 
                     ?? throw new InvalidOperationException("Overdraft response was empty.");

        return result.Overdraft;
    }
}
