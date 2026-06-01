
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
        PactBuilder
            .UponReceiving("a request to get account balance")
            .Given(ProviderStates.Accounts.AccountExistsForBalance)
            .WithRequest(HttpMethod.Get, "/api/accounts/1/balance")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                accountId = Match.Integer(1),
                balance = Match.Decimal(1250.75m),
                currency = Match.Type("USD")    
            });
        
        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var balance = await client.GetBalanceAsync(1);

            Assert.Equal(1250.75m, balance);
        });
    }

    [Fact]
    public async Task GetBalance_AccountNotFound_ThrowsAccountNotFoundException()
    {


        var expectedResponse = new
        {
            message = "Account not found"
        };  

        PactBuilder
            .UponReceiving("a request to get balance for non-existing account")
            .Given(ProviderStates.Accounts.AccountNotFound)
            .WithRequest(HttpMethod.Get, "/api/accounts/999/balance")
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
                () => client.GetBalanceAsync(999));
            Assert.Equal(expectedResponse.message, ex.Message);
        });
    }
}
