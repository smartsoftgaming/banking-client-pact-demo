using BankingPlatform.Client.Clients;
using Microsoft.Net.Http.Headers;
using PactNet.Matchers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using Xunit.Abstractions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public record UserSuccessTestData(
    int UserId,
    string ExpectedUsername,
    string ExpectedEmail,
    string EmailRegex,
    string Description);

public class GetUserTests(ITestOutputHelper output) : PactTestBase(output)
{
    public static TheoryData<UserSuccessTestData> SuccessTestCases => new()
    {
        new UserSuccessTestData(1, "test", "test@test.com", ".+@.+", "a request to get user details")
    };

    [Theory]
    [MemberData(nameof(SuccessTestCases))]
    public async Task GetUser_ExistingUser_ReturnsUser(UserSuccessTestData testData)
    {
        PactBuilder
            .UponReceiving(testData.Description)
            .Given(ProviderStates.Users.UserExists)
            .WithRequest(HttpMethod.Get, $"/api/users/{testData.UserId}/details")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                userId = Match.Integer(testData.UserId),
                userName = testData.ExpectedUsername,
                email = Match.Regex(testData.ExpectedEmail, testData.EmailRegex)
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new UserApiClient(httpClient);
            var user = await client.GetUserAsync(testData.UserId);

            Assert.IsType<int>(user.UserId);
            Assert.Equal(testData.ExpectedUsername, user.UserName);
            Assert.Equal(testData.ExpectedEmail, user.Email);
        });
    }
}
