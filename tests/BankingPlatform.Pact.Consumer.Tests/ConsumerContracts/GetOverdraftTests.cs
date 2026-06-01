using BankingPlatform.Client.Clients;
using BankingPlatform.Client.Exceptions;
using Microsoft.Net.Http.Headers;
using PactNet.Matchers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using Xunit.Abstractions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public record OverdraftSuccessTestData(
    int AccountId,
    decimal ExpectedOverdraft,
    string Currency,
    string Description);

public record OverdraftNotFoundTestData(
    int AccountId,
    string ExpectedMessage,
    string Description);

public class GetOverdraftTests(ITestOutputHelper output) : PactTestBase(output)
{
    public static TheoryData<OverdraftSuccessTestData> SuccessTestCases => new()
    {
        new OverdraftSuccessTestData(1, 300.00m, "USD", "a request to get account overdraft")
    };

    public static TheoryData<OverdraftNotFoundTestData> NotFoundTestCases => new()
    {
        new OverdraftNotFoundTestData(999, "Account not found", "a request to get overdraft for non-existing account")
    };

    [Theory]
    [MemberData(nameof(SuccessTestCases))]
    public async Task GetOverdraft_ExistingAccount_ReturnsOverdraft(OverdraftSuccessTestData testData)
    {
        PactBuilder
            .UponReceiving(testData.Description)
            .Given(ProviderStates.Accounts.AccountExistsForOverdraft)
            .WithRequest(HttpMethod.Get, $"/api/accounts/{testData.AccountId}/overdraft")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                accountId = Match.Integer(testData.AccountId),
                overdraft = Match.Decimal(testData.ExpectedOverdraft),
                currency = Match.Type(testData.Currency)
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var overdraft = await client.GetOverdraftAsync(testData.AccountId);

            Assert.Equal(testData.ExpectedOverdraft, overdraft);
        });
    }

    [Theory]
    [MemberData(nameof(NotFoundTestCases))]
    public async Task GetOverdraft_AccountNotFound_ThrowsAccountNotFoundException(OverdraftNotFoundTestData testData)
    {
        PactBuilder
            .UponReceiving(testData.Description)
            .Given(ProviderStates.Accounts.AccountNotFound)
            .WithRequest(HttpMethod.Get, $"/api/accounts/{testData.AccountId}/overdraft")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.NotFound)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new { message = testData.ExpectedMessage });

        await PactBuilder.VerifyAsync(async ctx =>  
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new AccountApiClient(httpClient);
            var ex = await Assert.ThrowsAsync<NotFoundException>(
                () => client.GetOverdraftAsync(testData.AccountId));

            Assert.Equal(testData.ExpectedMessage, ex.Message);
        });
    }
}
