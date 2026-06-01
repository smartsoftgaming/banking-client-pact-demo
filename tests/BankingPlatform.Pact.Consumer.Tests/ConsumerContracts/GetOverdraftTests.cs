using BankingPlatform.Client.Clients;
using BankingPlatform.Client.Exceptions;
using Microsoft.Net.Http.Headers;
using PactNet.Matchers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
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
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                accountId = Match.Integer(1),
                overdraft = Match.Decimal(300.00m),
                currency = Match.Type("USD")
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var overdraft = await client.GetOverdraftAsync(1);

            Assert.Equal(300.00m, overdraft);
        });
    }

    [Fact]
    public async Task GetOverdraft_AccountNotFound_ThrowsAccountNotFoundException()
    {
        var expectedResponse = new
        {
            message = "Account not found"
        };

        PactBuilder
            .UponReceiving("a request to get overdraft for non-existing account")
            .Given(ProviderStates.Accounts.AccountNotFound)
            .WithRequest(HttpMethod.Get, "/api/accounts/999/overdraft")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(expectedResponse);

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => client.GetOverdraftAsync(999));

            Assert.Equal(expectedResponse.message   , ex.Message);
        });
    }
}
