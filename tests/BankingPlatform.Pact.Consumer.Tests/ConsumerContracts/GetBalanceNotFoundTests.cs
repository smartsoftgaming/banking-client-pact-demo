using System.Net;
using Xunit.Abstractions;
using System.Net.Http.Headers;
using BankingPlatform.Client.Clients;
using BankingPlatform.Client.Exceptions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public class GetBalanceNotFoundTests(ITestOutputHelper output) : PactTestBase(output)
{
    [Fact]
    public async Task GetBalance_AccountNotFound_ThrowsAccountNotFoundException()
    {

        PactBuilder
            .UponReceiving("a request to get balance for non-existing account")
            .Given(ProviderStates.Accounts.AccountNotFound)
            .WithRequest(HttpMethod.Get, "/api/accounts/999/balance")
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
                () => client.GetBalanceAsync(999));
            Assert.Equal("Account not found", ex.Message);
        });     
    }
}

    