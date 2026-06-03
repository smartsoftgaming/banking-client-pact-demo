using BankingPlatform.Client.Exceptions;
using BankingPlatform.Client.Models;
using System.Net;
using System.Net.Http.Json;

namespace BankingPlatform.Client.Clients;

public interface IUserApiClient
{
    Task<UserResponse> GetUserAsync(int userId, CancellationToken cancellationToken = default);

}

public class UserApiClient(HttpClient httpClient) : IUserApiClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<UserResponse> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"/api/users/{userId}/details", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>(cancellationToken: cancellationToken);
            throw new NotFoundException(error?.Message ?? "User not found");
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<UserResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("User response was empty.");
    }
}