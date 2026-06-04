using BankingPlatform.Client.Clients;
using BankingPlatform.Client.Exceptions;
using Microsoft.Net.Http.Headers;
using PactNet.Matchers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using Xunit.Abstractions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public class GetBalanceTests(ITestOutputHelper output) : PactTestBase(output)
{
    [Fact]
    public async Task GetBalance_ExistingAccount_ReturnsBalance()
    {
        const int AccountId = 1;
        const decimal ExpectedBalance = 1250.75m;   

        PactBuilder
            .UponReceiving("a request to get account balance")
            .Given(ProviderStates.Accounts.AccountExistsForBalance)
            .WithRequest(HttpMethod.Get, $"/api/accounts/{AccountId}/balance")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                accountId = AccountId,
                balance = ExpectedBalance,
                currency = "USD"
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var balance = await client.GetBalanceAsync(AccountId);

            Assert.Equal(ExpectedBalance, balance);
        });

    }

    [Fact]
    public async Task GetBalance_AccountNotFound_ThrowsAccountNotFoundException()
    {
        const int NonExistingAccountId = 999;
        const string ExpectedErrorMessage = "Account not found";

        PactBuilder
            .UponReceiving("a request to get balance for non-existing account")
            .Given(ProviderStates.Accounts.AccountNotFound)
            .WithRequest(HttpMethod.Get, $"/api/accounts/{NonExistingAccountId}/balance")
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
                () => client.GetBalanceAsync(NonExistingAccountId));

            Assert.Equal(ExpectedErrorMessage, ex.Message);
        });
    }
}
