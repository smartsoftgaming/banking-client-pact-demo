using PactNet;
using PactNet.Infrastructure.Outputters;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using Xunit.Abstractions;   

namespace BankingPlatform.Pact.Consumer.Tests;  

public abstract class PactTestBase
{
    protected const string PactConsumerName = "BankingPlatformConsumer";
    protected const string PactProviderName = "BankingAppProvider";

    protected readonly IPactBuilderV4 PactBuilder;

    protected static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    protected PactTestBase(ITestOutputHelper output)
    {
        var pactDir = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..", "..", "..", "..", "..",
            "pacts");

        Directory.CreateDirectory(pactDir);

        var config = new PactConfig
        {
            PactDir = pactDir,
            Outputters = [new XUnitOutput(output)],
            LogLevel = PactLogLevel.Information,
            DefaultJsonSettings = JsonOptions
        };

        var pact = PactNet.Pact.V4(PactConsumerName, PactProviderName, config);
        PactBuilder = pact.WithHttpInteractions();
    }

    protected static HttpClient CreateHttpClient(Uri baseAddress)
    {
        var httpClient = new HttpClient { BaseAddress = baseAddress };
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        return httpClient;
    }
}

public class XUnitOutput(ITestOutputHelper output) : IOutput
{
    private readonly ITestOutputHelper _output = output;

    public void WriteLine(string line)
    {
        _output.WriteLine(line);
    }
}