using BankingPlatform.Client.Clients;
using BankingPlatform.Client.Exceptions;
using Microsoft.Net.Http.Headers;
using PactNet.Matchers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using Xunit.Abstractions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public record BalanceSuccessTestData(
    int AccountId,
    decimal ExpectedBalance,
    string Currency,
    string Description);

public record BalanceNotFoundTestData(
    int AccountId,
    string ExpectedMessage,
    string Description);

public class GetBalanceTests(ITestOutputHelper output) : PactTestBase(output)
{
    public static TheoryData<BalanceSuccessTestData> SuccessTestCases => new()
    {
        new BalanceSuccessTestData(1, 1250.75m, "USD", "a request to get account balance")
    };

    public static TheoryData<BalanceNotFoundTestData> NotFoundTestCases => new()
    {
        new BalanceNotFoundTestData(999, "Account not found", "a request to get balance for non-existing account")
    };

    [Theory]
    [MemberData(nameof(SuccessTestCases))]
    public async Task GetBalance_ExistingAccount_ReturnsBalance(BalanceSuccessTestData testData)
    {
        PactBuilder
            .UponReceiving(testData.Description)
            .Given(ProviderStates.Accounts.AccountExistsForBalance)
            .WithRequest(HttpMethod.Get, $"/api/accounts/{testData.AccountId}/balance")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                accountId = Match.Integer(testData.AccountId),
                balance = Match.Decimal(testData.ExpectedBalance),
                currency = Match.Type(testData.Currency)
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var balance = await client.GetBalanceAsync(testData.AccountId);

            Assert.Equal(testData.ExpectedBalance, balance);
        });
    }

    [Theory]
    [MemberData(nameof(NotFoundTestCases))]
    public async Task GetBalance_AccountNotFound_ThrowsAccountNotFoundException(BalanceNotFoundTestData testData)
    {
        var expectedResponse = new
        {
            message = testData.ExpectedMessage
        };

        PactBuilder
            .UponReceiving(testData.Description)
            .Given(ProviderStates.Accounts.AccountNotFound)
            .WithRequest(HttpMethod.Get, $"/api/accounts/{testData.AccountId}/balance")
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
                () => client.GetBalanceAsync(testData.AccountId));

            Assert.Equal(expectedResponse.message, ex.Message);
        });
    }
}
