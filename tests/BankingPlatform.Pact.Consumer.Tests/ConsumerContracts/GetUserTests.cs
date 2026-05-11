using System.Net;
using System.Net.Http.Headers;
using BankingPlatform.Client.Clients;
using PactNet.Matchers;
using Xunit.Abstractions;

namespace BankingPlatform.Pact.Consumer.Tests.ConsumerContracts;

public class GetUserTests(ITestOutputHelper output) : PactTestBase(output)
{
    [Fact]
    public async Task GetUser_ExistingUser_ReturnsUser()
    {
        PactBuilder
            .UponReceiving("a request to get user details")
            .Given(ProviderStates.Users.UserExists)
            .WithRequest(HttpMethod.Get, "/api/users/1/details")
            .WithHeader("Accept", "application/json")
            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader("Content-Type", "application/json; charset=utf-8")
            .WithJsonBody(new
            {
                userId = Match.Integer(1),
                username = Match.Type("test"),
                email = Match.Regex("test@test.com", ".+@.+")
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
            var httpClient = new HttpClient
            {
                BaseAddress = ctx.MockServerUri
            };

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var client = new UserApiClient(httpClient);

            var user = await client.GetUserAsync(1);

            Assert.IsType<int>(user.UserId);
            Assert.Equal("test", user.Username);
            Assert.Equal("test@test.com", user.Email);
        });
    }
}
