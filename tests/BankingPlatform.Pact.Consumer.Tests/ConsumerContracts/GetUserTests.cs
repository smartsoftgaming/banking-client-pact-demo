using BankingPlatform.Client.Clients;
using Microsoft.Net.Http.Headers;
using PactNet.Matchers;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
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
            .WithHeader(HeaderNames.Accept, MediaTypeNames.Application.Json)

            .WillRespond()
            .WithStatus(HttpStatusCode.OK)
            .WithHeader(HeaderNames.ContentType, MediaTypeNames.Application.Json)
            .WithJsonBody(new
            {
                userId = Match.Integer(1),
                username = Match.Type("test"),
                email = Match.Regex("test@test.com", ".+@.+")
            });

        await PactBuilder.VerifyAsync(async ctx =>
        {
        

            var httpClient = CreateHttpClient(ctx.MockServerUri);
            var client = new UserApiClient(httpClient);
            var user = await client.GetUserAsync(1);

            Assert.IsType<int>(user.UserId);
            Assert.Equal("test", user.Username);
            Assert.Equal("test@test.com", user.Email);
        });
    }
}
