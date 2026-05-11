using System.Net;
using System.Net.Http.Headers;
using BankingPlatform.Client.Clients;
using BankingPlatform.Client.Exceptions;
using Xunit.Abstractions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public class GetOverdraftNotFoundTests(ITestOutputHelper output) : PactTestBase(output)
{
    [Fact]
    public async Task GetOverdraft_AccountNotFound_ThrowsAccountNotFoundException()
    {
        PactBuilder
            .UponReceiving("a request to get overdraft for non-existing account")
            .Given(ProviderStates.Accounts.AccountNotFound)
            .WithRequest(HttpMethod.Get, "/api/accounts/999/overdraft")
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new { message = "Account not found" });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = new HttpClient { BaseAddress = ctx.MockServerUri };
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var client = new AccountApiClient(httpClient);
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => client.GetOverdraftAsync(999));

            Assert.Equal("Account not found", ex.Message);
        });
    }
}


