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
        const int AccountId = 1;
        const decimal ExpectedOverdraft = 300.00m;

        PactBuilder
            .UponReceiving("a request to get account overdraft")
            .Given(ProviderStates.Accounts.AccountExistsForOverdraft)
            .WithRequest(HttpMethod.Get, $"/api/accounts/{AccountId}/overdraft")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                accountId = AccountId,
                overdraft = ExpectedOverdraft,
                currency = "USD"
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var overdraft = await client.GetOverdraftAsync(AccountId);

            Assert.Equal(ExpectedOverdraft, overdraft);
        });
    }

    [Fact]
    public async Task GetOverdraft_AccountNotFound_ThrowsAccountNotFoundException()
    {
        const int NonExistingAccountId = 999;
        const string ExpectedErrorMessage = "Account not found";

        PactBuilder
            .UponReceiving("a request to get overdraft for non-existing account")
            .Given(ProviderStates.Accounts.AccountNotFound)
            .WithRequest(HttpMethod.Get, $"/api/accounts/{NonExistingAccountId}/overdraft")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new { message = ExpectedErrorMessage });

        await PactBuilder.VerifyAsync(async ctx =>  
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => client.GetOverdraftAsync(NonExistingAccountId));

            Assert.Equal(ExpectedErrorMessage, ex.Message);
        });
    }
}
