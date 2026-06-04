using BankingPlatform.Client.Clients;
using Microsoft.Net.Http.Headers;
using PactNet.Matchers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using Xunit.Abstractions;
using static System.Net.Mime.MediaTypeNames;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public class GetUserTests(ITestOutputHelper output) : PactTestBase(output)
{

    [Fact]
    public async Task GetUser_ExistingUser_ReturnsUser()
    {        
        const int UserId = 1;
        const string ExpectedUserName = "test";
        const string ExpectedEmail = "test@test.com";

        PactBuilder
            .UponReceiving("a request to get user details")
            .Given(ProviderStates.Users.UserExists)
            .WithRequest(HttpMethod.Get, $"/api/users/{UserId}/details")
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                userId = Match.Integer(UserId),
                userName = ExpectedUserName,
                email = Match.Regex(ExpectedEmail, ".+@.+")
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new UserApiClient(httpClient);
            var user = await client.GetUserAsync(UserId);

            Assert.Equal(UserId, user.UserId);
            Assert.Equal(ExpectedUserName, user.UserName);
            Assert.Equal(ExpectedEmail, user.Email);
        });
    }
}
