using System.Net;
using System.Net.Http.Headers;
using BankingPlatform.Client.Clients;
using PactNet.Matchers;
using Xunit.Abstractions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public class GetOverdraftTests(ITestOutputHelper output) : PactTestBase(output)
{
    [Fact]
    public async Task GetOverdraft_ExistingAccount_ReturnsOverdraft()
    {

        PactBuilder
            .UponReceiving("a request to get account overdraft")
            .Given(ProviderStates.Accounts.AccountExistsForOverdraft)
            .WithRequest(HttpMethod.Get, "/api/accounts/1/overdraft")
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                accountId = Match.Integer(1),
                overdraft = Match.Decimal(300.00m),
                currency = Match.Type("USD")
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = new HttpClient { BaseAddress = ctx.MockServerUri };
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var client = new AccountApiClient(httpClient);
            var overdraft = await client.GetOverdraftAsync(1);

            Assert.Equal(300.00m, overdraft);
        });
    }
}
