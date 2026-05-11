
using PactNet.Matchers;
using System.Net;
using System.Net.Http.Headers;
using BankingPlatform.Client.Clients;
using Xunit.Abstractions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public class GetBalanceTests(ITestOutputHelper output) : PactTestBase(output)
{
    [Fact]
    public async Task GetBalance_ExistingAccount_ReturnsBalance()
    {
        PactBuilder
            .UponReceiving("a request to get account balance")
            .Given(ProviderStates.Accounts.AccountExistsForBalance)
            .WithRequest(HttpMethod.Get, "/api/accounts/1/balance")
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                accountId = Match.Integer(1),
                balance = Match.Decimal(1250.75m),
                currency = Match.Type("USD")    
            });
        
        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = new HttpClient { BaseAddress = ctx.MockServerUri };
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var client = new AccountApiClient(httpClient);
            var balance = await client.GetBalanceAsync(1);

            Assert.Equal(1250.75m, balance);
        });
    }
}
